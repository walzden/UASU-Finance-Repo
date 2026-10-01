using Dapper;
using Microsoft.Data.SqlClient;
using UASU_VoucherApprovals.Data;
using UASU_VoucherApprovals.Models;

namespace UASU_VoucherApprovals.Services;

// Raises one Expense voucher per selected current official for the same
// budget line and description (e.g. a sitting allowance paid to most of
// the committee), instead of entering each on New Voucher by hand. Each
// voucher is an ordinary voucher afterwards and still needs Chairman and
// Chapter Secretary approval.
public interface IBulkVoucherService
{
    Task<IEnumerable<BulkVoucherCandidateRow>> GetCandidatesAsync(string? description);
    Task<IReadOnlyList<string>> CreateAsync(DateTime voucherDate, string budgetLink, string description,
        IReadOnlyDictionary<string, decimal> amountsByOfficial);
}

public class BulkVoucherService : IBulkVoucherService
{
    private readonly IDbConnectionFactory _db;

    public BulkVoucherService(IDbConnectionFactory db)
    {
        _db = db;
    }

    // Shared by the page and CreateAsync so the same rules decide who
    // is listed and who can be paid. Rejected vouchers don't count - a
    // rejected one is meant to be corrected, and its replacement should
    // be allowed through (same rule as HonorariaService).
    private const string CandidatesSql = @"
        SELECT o.OfficialID, o.FullName, o.Role,
            CAST(CASE WHEN EXISTS (
                SELECT 1 FROM Vouchers v
                WHERE v.Official_Link = o.OfficialID
                  AND v.[Description] = @Description
                  AND NOT EXISTS (
                      SELECT 1 FROM Voucher_Approvals va
                      WHERE va.Voucher_ID = v.Voucher_ID AND va.Approval_Status = 'Rejected')
            ) THEN 1 ELSE 0 END AS bit) AS HasVoucher
        FROM Ref_Officials o
        WHERE o.IsCurrent = 1
        ORDER BY o.FullName;";

    public async Task<IEnumerable<BulkVoucherCandidateRow>> GetCandidatesAsync(string? description)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<BulkVoucherCandidateRow>(
            CandidatesSql, new { Description = description?.Trim() ?? string.Empty });
    }

    // All-or-nothing: one transaction, so a trigger or constraint
    // rejecting one voucher leaves no half-created batch behind. The
    // officials are re-checked here rather than trusted from the form:
    // anyone no longer current, or who already has a voucher with this
    // description, is refused rather than silently skipped, so the
    // Treasurer never ends up with fewer vouchers than they confirmed.
    public async Task<IReadOnlyList<string>> CreateAsync(DateTime voucherDate, string budgetLink, string description,
        IReadOnlyDictionary<string, decimal> amountsByOfficial)
    {
        description = description.Trim();

        using var conn = (SqlConnection)_db.CreateConnection();
        conn.Open();
        using var transaction = conn.BeginTransaction();

        try
        {
            var budgetType = await conn.ExecuteScalarAsync<string?>(
                "SELECT Category_Type FROM Ref_BudgetCodes WHERE Budget_ID = @BudgetLink;",
                new { BudgetLink = budgetLink }, transaction);
            if (!string.Equals(budgetType, "Expense", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Choose an Expense budget code for these vouchers.");

            var candidates = (await conn.QueryAsync<BulkVoucherCandidateRow>(
                    CandidatesSql, new { Description = description }, transaction))
                .ToDictionary(c => c.OfficialID);

            var problems = new List<string>();
            foreach (var officialId in amountsByOfficial.Keys)
            {
                if (!candidates.TryGetValue(officialId, out var c))
                    problems.Add($"{officialId} is not a current official");
                else if (c.HasVoucher)
                    problems.Add($"{c.FullName} already has a voucher described \"{description}\"");
            }
            if (problems.Count > 0)
                throw new InvalidOperationException("Nothing was created: " + string.Join("; ", problems) + ".");

            const string insertSql = @"
                INSERT INTO Vouchers
                    (VoucherDate, Transaction_Type, Budget_Link, Official_Link, Amount,
                     [Description], Payee_Category, Is_Travel_Expense)
                OUTPUT INSERTED.Voucher_ID
                VALUES
                    (@VoucherDate, 'Expense', @BudgetLink, @OfficialId, @Amount,
                     @Description, 'Official', 0);";

            var voucherIds = new List<string>();
            foreach (var c in candidates.Values.Where(c => amountsByOfficial.ContainsKey(c.OfficialID)))
            {
                var id = await conn.ExecuteScalarAsync<string>(insertSql, new
                {
                    VoucherDate = voucherDate,
                    BudgetLink = budgetLink,
                    OfficialId = c.OfficialID,
                    Amount = amountsByOfficial[c.OfficialID],
                    Description = description
                }, transaction);
                voucherIds.Add(id!);
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
