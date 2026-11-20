using Dapper;
using UASU_VoucherApprovals.Data;
using UASU_VoucherApprovals.Models;

namespace UASU_VoucherApprovals.Services;

public interface IDebtService
{
    Task<IReadOnlyList<string>> CreateDebtsAsync(IReadOnlyList<NewDebt> debts, DateTime dateIncurred,
        DateTime? dateIncurredTo, string? invoiceNo, string description);
    Task<IEnumerable<DebtOption>> GetOutstandingDebtsAsync();
    Task<IEnumerable<DebtStatusRow>> GetDebtStatusReportAsync();
    Task<DebtDetail?> GetDebtDetailAsync(string debtId);
}

public class DebtService : IDebtService
{
    private readonly IDbConnectionFactory _db;

    public DebtService(IDbConnectionFactory db)
    {
        _db = db;
    }

    // One or more debts sharing a description, invoice and dates - a
    // supplier debt, a personal debt to one official, or a common debt
    // split across several officials (one row each, for their share). All
    // in one transaction, so a group debt is never half-recorded.
    public async Task<IReadOnlyList<string>> CreateDebtsAsync(IReadOnlyList<NewDebt> debts, DateTime dateIncurred,
        DateTime? dateIncurredTo, string? invoiceNo, string description)
    {
        using var conn = (Microsoft.Data.SqlClient.SqlConnection)_db.CreateConnection();
        conn.Open();
        using var transaction = conn.BeginTransaction();

        // Debt_ID is server-generated (DEFAULT expression using DebtSeq).
        // OUTPUT goes INTO a table variable so this keeps working if
        // Debt_Register ever gains an INSERT trigger.
        const string sql = @"
            DECLARE @Inserted TABLE (Debt_ID NVARCHAR(20));
            INSERT INTO Debt_Register
                (Creditor_Type, Official_link, Supplier_link, Date_Incurred, Date_Incurred_To, Invoice_No, [Description], Total_Owed)
            OUTPUT INSERTED.Debt_ID INTO @Inserted
            VALUES
                (@Creditor_Type, @Official_link, @Supplier_link, @DateIncurred, @DateIncurredTo, @InvoiceNo, @Description, @Total_Owed);
            SELECT Debt_ID FROM @Inserted;";

        try
        {
            var ids = new List<string>();
            foreach (var d in debts)
            {
                ids.Add((await conn.ExecuteScalarAsync<string>(sql, new
                {
                    d.Creditor_Type,
                    d.Official_link,
                    d.Supplier_link,
                    d.Total_Owed,
                    DateIncurred = dateIncurred.Date,
                    DateIncurredTo = dateIncurredTo?.Date,
                    InvoiceNo = string.IsNullOrWhiteSpace(invoiceNo) ? null : invoiceNo.Trim(),
                    Description = description.Trim()
                }, transaction))!);
            }
            transaction.Commit();
            return ids;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public async Task<IEnumerable<DebtOption>> GetOutstandingDebtsAsync()
    {
        using var conn = _db.CreateConnection();

        // DebtSummary computes Balance = Total_Owed - TotalAllocated in
        // the same query, so filtering on Balance > 0 directly can't
        // drift out of sync the way filtering on SettlementStatus could
        // if Is_Settled (a flag set separately, by a trigger) ever fell
        // behind the actual running balance - only debts that
        // genuinely still owe money are offered here.
        // Only debts with something still to raise a voucher for: what's
        // owed less what live (not rejected, not voided) vouchers already
        // cover, paid or not - fn_DebtCommitments (SQL/035), the same
        // definition trg_Vouchers_DebtNotOverCommitted enforces. Without
        // this, a debt with a voucher awaiting approval stayed listed and
        // could be raised a second time.
        const string sql = @"
            SELECT ds.Debt_ID AS Id,
                   ds.Debt_ID + ' - ' + COALESCE(o.FullName, s.Business_Name, 'Unknown')
                   + ' - Sh.' + FORMAT(ds.Total_Owed - ISNULL(c.Committed, 0),'N2') + ' to raise'
                   + CASE WHEN c.Committed > 0 THEN ' (of Sh.' + FORMAT(ds.Total_Owed,'N2') + ')' ELSE '' END AS Label,
                   ds.[Description],
                   ds.Total_Owed - ISNULL(c.Committed, 0) AS Remaining
            FROM DebtSummary ds
            LEFT JOIN dbo.fn_DebtCommitments() c ON c.Debt_ID = ds.Debt_ID
            LEFT JOIN Ref_Officials o ON o.OfficialID = ds.Official_Link
            LEFT JOIN Ref_Suppliers s ON s.Supplier_ID = ds.Supplier_Link
            WHERE ds.Balance > 0
              AND ds.Total_Owed - ISNULL(c.Committed, 0) > 0
            ORDER BY ds.Date_Incurred;";

        return await conn.QueryAsync<DebtOption>(sql);
    }

    public async Task<IEnumerable<DebtStatusRow>> GetDebtStatusReportAsync()
    {
        using var conn = _db.CreateConnection();

        return await conn.QueryAsync<DebtStatusRow>(DebtStatusSql + " ORDER BY ds.Date_Incurred;");
    }

    // Shared by the report and the detail page so a debt's figures are
    // identical on both.
    private const string DebtStatusSql = @"
        SELECT ds.Debt_ID, ds.Creditor_Type,
               COALESCE(o.FullName, s.Business_Name, 'Unknown') AS CreditorName,
               ds.Date_Incurred, ds.Date_Incurred_To, ds.Invoice_No, ds.[Description],
               ds.Total_Owed, ds.TotalAllocated, ds.Balance, ds.SettlementStatus,
               ISNULL(c.Committed, 0) AS Committed
        FROM DebtSummary ds
        LEFT JOIN dbo.fn_DebtCommitments() c ON c.Debt_ID = ds.Debt_ID
        LEFT JOIN Ref_Officials o ON o.OfficialID = ds.Official_Link
        LEFT JOIN Ref_Suppliers s ON s.Supplier_ID = ds.Supplier_Link";

    // Everything behind one debt's figures: every voucher ever raised for
    // it (including rejected and voided ones, for the record) and every
    // payment against those vouchers.
    public async Task<DebtDetail?> GetDebtDetailAsync(string debtId)
    {
        using var conn = _db.CreateConnection();

        var debt = await conn.QuerySingleOrDefaultAsync<DebtStatusRow>(
            DebtStatusSql + " WHERE ds.Debt_ID = @DebtId;", new { DebtId = debtId });
        if (debt is null)
            return null;

        const string vouchersSql = @"
            WITH links AS (
                -- Direct link (New Voucher); Pay Debts vouchers are in the
                -- second branch even when Debt_Link is also set.
                SELECT v.Voucher_ID, v.Amount AS DebtShare, CAST(0 AS bit) AS SharedWithOtherDebts
                FROM Vouchers v
                WHERE v.Debt_Link = @DebtId
                  AND NOT EXISTS (SELECT 1 FROM VoucherDebts vd WHERE vd.Voucher_ID = v.Voucher_ID)
                UNION ALL
                SELECT vd.Voucher_ID, vd.Amount,
                       CAST(CASE WHEN EXISTS (SELECT 1 FROM VoucherDebts o
                                              WHERE o.Voucher_ID = vd.Voucher_ID AND o.Debt_ID <> vd.Debt_ID)
                                 THEN 1 ELSE 0 END AS bit)
                FROM VoucherDebts vd
                WHERE vd.Debt_ID = @DebtId
            )
            SELECT v.Voucher_ID, v.VoucherDate, v.[Description], v.Amount AS VoucherAmount,
                   l.DebtShare, l.SharedWithOtherDebts,
                   ISNULL(p.Paid, 0) AS PaidOnVoucher,
                   CASE
                       WHEN v.Voided_Date IS NOT NULL THEN 'Voided'
                       WHEN rj.Voucher_ID IS NOT NULL THEN 'Rejected'
                       WHEN ISNULL(p.Paid, 0) >= v.Amount THEN 'Paid'
                       WHEN ISNULL(p.Paid, 0) > 0 THEN 'Partially Paid'
                       WHEN vas.FullyApproved = 1 THEN 'Approved - not paid'
                       ELSE 'Awaiting approval'
                   END AS Status,
                   CASE
                       WHEN v.Voided_Date IS NOT NULL THEN v.Void_Reason
                       WHEN rj.Voucher_ID IS NOT NULL THEN rj.Comments
                       WHEN vas.FullyApproved = 0 AND (vas.ChairmanApproved = 1 OR vas.ChapterSecretaryApproved = 1)
                           THEN CASE WHEN vas.ChairmanApproved = 1 THEN 'Chairman approved' ELSE 'Chapter Secretary approved' END
                   END AS StatusNote
            FROM links l
            INNER JOIN Vouchers v ON v.Voucher_ID = l.Voucher_ID
            LEFT JOIN VoucherApprovalStatus vas ON vas.Voucher_ID = v.Voucher_ID
            OUTER APPLY (SELECT SUM(pa.Allocated_Amount) AS Paid FROM PaymentAllocations pa WHERE pa.Voucher_ID = v.Voucher_ID) p
            OUTER APPLY (SELECT TOP 1 va.Voucher_ID, va.Comments FROM Voucher_Approvals va
                         WHERE va.Voucher_ID = v.Voucher_ID AND va.Approval_Status = 'Rejected'
                         ORDER BY va.Approval_Date DESC) rj
            ORDER BY v.VoucherDate, v.Voucher_ID;";

        var vouchers = (await conn.QueryAsync<DebtVoucherRow>(vouchersSql, new { DebtId = debtId })).ToList();

        const string paymentsSql = @"
            SELECT pa.Voucher_ID, p.Payment_ID, p.Payment_Date, p.Payment_Mode, p.Reference_No, pa.Allocated_Amount
            FROM PaymentAllocations pa
            INNER JOIN Payments p ON p.Payment_ID = pa.Payment_ID
            WHERE pa.Voucher_ID IN @Ids
            ORDER BY p.Payment_Date, p.Payment_ID;";

        var payments = vouchers.Count == 0
            ? new List<DebtPaymentRow>()
            : (await conn.QueryAsync<DebtPaymentRow>(paymentsSql, new { Ids = vouchers.Select(v => v.Voucher_ID) })).ToList();

        return new DebtDetail { Debt = debt, Vouchers = vouchers, Payments = payments };
    }
}
