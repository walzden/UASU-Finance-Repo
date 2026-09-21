using Dapper;
using Microsoft.Data.SqlClient;
using UASU_VoucherApprovals.Data;
using UASU_VoucherApprovals.Models;
using UASU_VoucherApprovals.Utilities;

namespace UASU_VoucherApprovals.Services;

// Raises vouchers from outstanding debts owed to officials (Pay Debts page).
// One voucher can pay several debts as long as they are all owed to the same
// official; because a voucher carries a single budget item, one official's
// debts under different budget items become separate vouchers.
public interface IDebtPaymentService
{
    Task<IReadOnlyList<DebtToPay>> GetPayableDebtsAsync();

    (List<(DebtToPay Debt, DebtSelectionInput Selection)> Selected, List<string> Errors)
        ValidateSelection(IEnumerable<DebtSelectionInput> rows, IReadOnlyList<DebtToPay> payable, IReadOnlyDictionary<string, string> budgetLabels, string preparedBy);

    List<VoucherPlan> PlanVouchers(IEnumerable<(DebtToPay Debt, DebtSelectionInput Selection)> selected, IReadOnlyDictionary<string, string> budgetLabels);

    Task<IReadOnlyList<string>> CreateVouchersAsync(IReadOnlyList<DebtSelectionInput> rows, string preparedBy, DateTime voucherDate, IReadOnlyDictionary<string, string> budgetLabels);
}

public class DebtPaymentService : IDebtPaymentService
{
    private readonly IDbConnectionFactory _db;

    public DebtPaymentService(IDbConnectionFactory db)
    {
        _db = db;
    }

    // Outstanding debts owed to officials that no live voucher covers yet.
    // "Live" = not rejected: a rejected voucher no longer counts, so its
    // debts come back here. Debts already part-paid through the older
    // one-debt-per-voucher flow are left to that flow.
    private const string PayableDebtsSql = @"
        SELECT ds.Debt_ID, ds.Official_Link AS OfficialID, o.FullName, o.Role,
               CASE WHEN u.LastLogin IS NULL THEN 1 ELSE 0 END AS NotSignedIn,
               ds.Date_Incurred, ds.[Description], ds.Total_Owed,
               p.Activity_ID, a.Category, dec.FullName AS DecidedByName
        FROM DebtSummary ds
        INNER JOIN Ref_Officials o ON o.OfficialID = ds.Official_Link
        LEFT JOIN Users u ON u.OfficialID = o.OfficialID
        LEFT JOIN ActivityParticipants p ON p.Debt_ID = ds.Debt_ID
        LEFT JOIN Activities a ON a.Activity_ID = p.Activity_ID
        LEFT JOIN Ref_Officials dec ON dec.OfficialID = p.Decided_By
        WHERE ds.Creditor_Type = 'Official'
          AND ds.TotalAllocated = 0
          AND ds.SettlementStatus <> 'Settled'
          AND NOT EXISTS (
                SELECT 1 FROM VoucherDebts vd
                WHERE vd.Debt_ID = ds.Debt_ID
                  AND NOT EXISTS (SELECT 1 FROM Voucher_Approvals va WHERE va.Voucher_ID = vd.Voucher_ID AND va.Approval_Status = 'Rejected'))
          AND NOT EXISTS (
                SELECT 1 FROM Vouchers v
                WHERE v.Debt_Link = ds.Debt_ID
                  AND NOT EXISTS (SELECT 1 FROM Voucher_Approvals va WHERE va.Voucher_ID = v.Voucher_ID AND va.Approval_Status = 'Rejected'))
        ORDER BY o.FullName, ds.Date_Incurred, ds.Debt_ID;";

    public async Task<IReadOnlyList<DebtToPay>> GetPayableDebtsAsync()
    {
        using var conn = _db.CreateConnection();
        return (await conn.QueryAsync<DebtToPay>(PayableDebtsSql)).ToList();
    }

    public (List<(DebtToPay Debt, DebtSelectionInput Selection)> Selected, List<string> Errors)
        ValidateSelection(IEnumerable<DebtSelectionInput> rows, IReadOnlyList<DebtToPay> payable, IReadOnlyDictionary<string, string> budgetLabels, string preparedBy)
    {
        var byId = payable.ToDictionary(d => d.Debt_ID);
        var selected = new List<(DebtToPay, DebtSelectionInput)>();
        var errors = new List<string>();

        foreach (var row in rows.Where(r => r.Selected))
        {
            if (!byId.TryGetValue(row.Debt_ID, out var debt))
            {
                errors.Add($"{row.Debt_ID}: this debt is no longer waiting for a voucher (someone may have raised one already).");
                continue;
            }

            var label = $"{debt.Debt_ID} ({debt.FullName})";

            if (debt.OfficialID == preparedBy)
                errors.Add($"{label}: you cannot raise a voucher for a debt owed to you; the other treasury officer must.");
            else if (string.IsNullOrWhiteSpace(row.Budget_Link) || !budgetLabels.ContainsKey(row.Budget_Link))
                errors.Add($"{label}: choose a budget item.");
            else
                selected.Add((debt, row));
        }

        return (selected, errors);
    }

    // One voucher per official per budget item.
    public List<VoucherPlan> PlanVouchers(IEnumerable<(DebtToPay Debt, DebtSelectionInput Selection)> selected, IReadOnlyDictionary<string, string> budgetLabels)
    {
        return selected
            .GroupBy(s => (s.Debt.OfficialID, Budget: s.Selection.Budget_Link!))
            .Select(g =>
            {
                var debts = g.Select(x => x.Debt).ToList();
                var (text, shortened) = ActivityDescription.Build(debts.Select(d =>
                    new DescriptionItem(d.Date_Incurred, string.IsNullOrWhiteSpace(d.Description) ? d.Debt_ID : d.Description!, d.Category ?? "Debts")));
                return new VoucherPlan
                {
                    OfficialID = g.Key.OfficialID,
                    OfficialName = debts[0].FullName,
                    OfficialRole = debts[0].Role,
                    Budget_Link = g.Key.Budget,
                    BudgetLabel = budgetLabels[g.Key.Budget],
                    Amount = debts.Sum(d => d.Total_Owed),
                    Description = text,
                    DescriptionShortened = shortened,
                    Debts = debts
                };
            })
            .OrderBy(v => v.OfficialName).ThenBy(v => v.BudgetLabel)
            .ToList();
    }

    // All-or-nothing: every voucher and its debt links are one transaction.
    // The debts are re-read inside it, so a debt that another treasury
    // officer raised a voucher for in the meantime is refused, not doubled
    // (trg_VoucherDebts_Validate checks the same thing again).
    public async Task<IReadOnlyList<string>> CreateVouchersAsync(IReadOnlyList<DebtSelectionInput> rows, string preparedBy, DateTime voucherDate, IReadOnlyDictionary<string, string> budgetLabels)
    {
        using var conn = (SqlConnection)_db.CreateConnection();
        conn.Open();
        using var transaction = conn.BeginTransaction();

        try
        {
            var payable = (await conn.QueryAsync<DebtToPay>(PayableDebtsSql, transaction: transaction)).ToList();
            var (selected, errors) = ValidateSelection(rows, payable, budgetLabels, preparedBy);
            if (errors.Count > 0)
                throw new InvalidOperationException(string.Join(" ", errors));
            if (selected.Count == 0)
                throw new InvalidOperationException("Tick at least one debt.");

            const string insertVoucher = @"
                INSERT INTO Vouchers
                    (VoucherDate, Transaction_Type, Budget_Link, Official_Link, Amount,
                     [Description], Payee_Category, Debt_link, Is_Travel_Expense)
                OUTPUT INSERTED.Voucher_ID
                VALUES
                    (@VoucherDate, 'Expense', @BudgetLink, @OfficialId, @Amount,
                     @Description, 'Official', @DebtLink, 0);";

            var voucherIds = new List<string>();
            foreach (var plan in PlanVouchers(selected, budgetLabels))
            {
                // A voucher paying exactly one debt also keeps Debt_link set, so
                // the existing single-debt screens and reports see it as before.
                var voucherId = await conn.ExecuteScalarAsync<string>(insertVoucher, new
                {
                    VoucherDate = voucherDate.Date,
                    BudgetLink = plan.Budget_Link,
                    OfficialId = plan.OfficialID,
                    plan.Amount,
                    plan.Description,
                    DebtLink = plan.Debts.Count == 1 ? plan.Debts[0].Debt_ID : null
                }, transaction);
                voucherIds.Add(voucherId!);

                // One multi-row INSERT: the trigger checks the debts add up to the
                // voucher amount once all of them are in.
                var parameters = new DynamicParameters();
                parameters.Add("VoucherId", voucherId);
                var values = new List<string>();
                for (var i = 0; i < plan.Debts.Count; i++)
                {
                    values.Add($"(@VoucherId, @Debt{i}, @Amount{i})");
                    parameters.Add($"Debt{i}", plan.Debts[i].Debt_ID);
                    parameters.Add($"Amount{i}", plan.Debts[i].Total_Owed);
                }

                await conn.ExecuteAsync(
                    "INSERT INTO VoucherDebts (Voucher_ID, Debt_ID, Amount) VALUES " + string.Join(", ", values) + ";",
                    parameters, transaction);
            }

            transaction.Commit();
            return voucherIds;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
}
