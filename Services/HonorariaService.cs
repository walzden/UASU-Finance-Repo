using Dapper;
using Microsoft.Data.SqlClient;
using UASU_VoucherApprovals.Data;
using UASU_VoucherApprovals.Models;

namespace UASU_VoucherApprovals.Services;

public interface IHonorariaService
{
    Task<IEnumerable<HonorariaRateRow>> GetRatesAsync();
    Task SetRateAsync(string role, decimal amount);
    Task DeleteRateAsync(string role);

    Task<IEnumerable<HonorariaPreviewRow>> GetPreviewAsync(string description);
    Task<IReadOnlyList<string>> GenerateAsync(string description, DateTime voucherDate, string budgetLink);
}

public class HonorariaService : IHonorariaService
{
    private readonly IDbConnectionFactory _db;

    public HonorariaService(IDbConnectionFactory db)
    {
        _db = db;
    }

    public async Task<IEnumerable<HonorariaRateRow>> GetRatesAsync()
    {
        using var conn = _db.CreateConnection();
        const string sql = "SELECT Role, Amount FROM Ref_HonorariaRates ORDER BY Amount DESC, Role;";
        return await conn.QueryAsync<HonorariaRateRow>(sql);
    }

    public async Task SetRateAsync(string role, decimal amount)
    {
        using var conn = _db.CreateConnection();
        const string sql = @"
            UPDATE Ref_HonorariaRates SET Amount = @Amount WHERE Role = @Role;
            IF @@ROWCOUNT = 0
                INSERT INTO Ref_HonorariaRates (Role, Amount) VALUES (@Role, @Amount);";
        await conn.ExecuteAsync(sql, new { Role = role, Amount = amount });
    }

    public async Task DeleteRateAsync(string role)
    {
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync("DELETE FROM Ref_HonorariaRates WHERE Role = @Role;", new { Role = role });
    }

    // Shared by the preview and GenerateAsync so what the Treasurer sees
    // is exactly what gets created. Rejected vouchers don't count as
    // "already generated" - a rejected one is meant to be corrected, and
    // its replacement should be allowed through.
    private const string PreviewSql = @"
        SELECT o.OfficialID, o.FullName, o.Role, r.Amount,
            CASE
                WHEN r.Amount IS NULL THEN 'NoRate'
                WHEN EXISTS (
                    SELECT 1 FROM Vouchers v
                    WHERE v.Official_Link = o.OfficialID
                      AND v.[Description] = @Description
                      AND NOT EXISTS (
                          SELECT 1 FROM Voucher_Approvals va
                          WHERE va.Voucher_ID = v.Voucher_ID AND va.Approval_Status = 'Rejected')
                ) THEN 'AlreadyGenerated'
                ELSE 'Ready'
            END AS Status
        FROM Ref_Officials o
        LEFT JOIN Ref_HonorariaRates r ON LTRIM(RTRIM(r.Role)) = LTRIM(RTRIM(o.Role))
        WHERE o.IsCurrent = 1
        ORDER BY CASE WHEN r.Amount IS NULL THEN 1 ELSE 0 END, r.Amount DESC, o.FullName;";

    public async Task<IEnumerable<HonorariaPreviewRow>> GetPreviewAsync(string description)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<HonorariaPreviewRow>(PreviewSql, new { Description = description });
    }

    // All-or-nothing: one transaction, so a failure partway (e.g. a
    // trigger or constraint rejecting one voucher) leaves no
    // half-generated month behind. The Ready set is recomputed here
    // rather than taken from the page, so amounts can never be tampered
    // with from the form.
    public async Task<IReadOnlyList<string>> GenerateAsync(string description, DateTime voucherDate, string budgetLink)
    {
        using var conn = (SqlConnection)_db.CreateConnection();
        conn.Open();
        using var transaction = conn.BeginTransaction();

        try
        {
            var ready = (await conn.QueryAsync<HonorariaPreviewRow>(
                    PreviewSql, new { Description = description }, transaction))
                .Where(r => r.Status == "Ready")
                .ToList();

            const string insertSql = @"
                INSERT INTO Vouchers
                    (VoucherDate, Transaction_Type, Budget_Link, Official_Link, Amount,
                     [Description], Payee_Category, Is_Travel_Expense)
                OUTPUT INSERTED.Voucher_ID
                VALUES
                    (@VoucherDate, 'Expense', @BudgetLink, @OfficialId, @Amount,
                     @Description, 'Official', 0);";

            var voucherIds = new List<string>();
            foreach (var row in ready)
            {
                var id = await conn.ExecuteScalarAsync<string>(insertSql, new
                {
                    VoucherDate = voucherDate,
                    BudgetLink = budgetLink,
                    OfficialId = row.OfficialID,
                    Amount = row.Amount,
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
