using Dapper;
using UASU_VoucherApprovals.Data;
using UASU_VoucherApprovals.Models;

namespace UASU_VoucherApprovals.Services;

public interface IDebtService
{
    Task<string> CreateDebtAsync(DebtInputModel input);
    Task<IEnumerable<DebtOption>> GetOutstandingDebtsAsync();
    Task<IEnumerable<DebtStatusRow>> GetDebtStatusReportAsync();
}

public class DebtService : IDebtService
{
    private readonly IDbConnectionFactory _db;

    public DebtService(IDbConnectionFactory db)
    {
        _db = db;
    }

    public async Task<string> CreateDebtAsync(DebtInputModel input)
    {
        using var conn = _db.CreateConnection();

        // Debt_ID is server-generated (DEFAULT expression using
        // DebtSeq), so OUTPUT INSERTED.Debt_ID hands the new number
        // straight back rather than re-querying for it.
        const string sql = @"
            INSERT INTO Debt_Register
                (Creditor_Type, Official_link, Supplier_link, Date_Incurred, Invoice_No, [Description], Total_Owed)
            OUTPUT INSERTED.Debt_ID
            VALUES
                (@Creditor_Type, @Official_link, @Supplier_link, @Date_Incurred, @Invoice_No, @Description, @Total_Owed);";

        return await conn.ExecuteScalarAsync<string>(sql, input);
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

        const string sql = @"
            SELECT ds.Debt_ID, ds.Creditor_Type,
                   COALESCE(o.FullName, s.Business_Name, 'Unknown') AS CreditorName,
                   ds.Date_Incurred, ds.Invoice_No, ds.[Description],
                   ds.Total_Owed, ds.TotalAllocated, ds.Balance, ds.SettlementStatus
            FROM DebtSummary ds
            LEFT JOIN Ref_Officials o ON o.OfficialID = ds.Official_Link
            LEFT JOIN Ref_Suppliers s ON s.Supplier_ID = ds.Supplier_Link
            ORDER BY ds.Date_Incurred;";

        return await conn.QueryAsync<DebtStatusRow>(sql);
    }
}
