using Dapper;
using UASU_VoucherApprovals.Data;
using UASU_VoucherApprovals.Models;

namespace UASU_VoucherApprovals.Services;

// CRUD for Ref_Officials/Ref_Suppliers/Ref_BudgetCodes - previously
// maintained by hand directly in SQL Server. All three tables are
// simple (no triggers, no CHECK constraints), so this is plain Dapper
// the same way every other service in this app works, just without an
// approval/payment workflow wrapped around it.
public interface IReferenceDataService
{
    Task<IEnumerable<OfficialRow>> GetOfficialsAsync();
    Task<string> CreateOfficialAsync(OfficialInputModel input);
    Task<OfficialInputModel?> GetOfficialForEditAsync(string officialId);
    Task UpdateOfficialAsync(string officialId, OfficialInputModel input);

    Task<IEnumerable<SupplierRow>> GetSuppliersAsync();
    Task<string> CreateSupplierAsync(SupplierInputModel input);
    Task<SupplierInputModel?> GetSupplierForEditAsync(string supplierId);
    Task UpdateSupplierAsync(string supplierId, SupplierInputModel input);

    Task<IEnumerable<BudgetCodeRow>> GetBudgetCodeRowsAsync();
    Task<string> CreateBudgetCodeAsync(BudgetCodeInputModel input);
    Task<BudgetCodeInputModel?> GetBudgetCodeForEditAsync(string budgetId);
    Task UpdateBudgetCodeAsync(string budgetId, BudgetCodeInputModel input);
}

public class ReferenceDataService : IReferenceDataService
{
    private readonly IDbConnectionFactory _db;

    public ReferenceDataService(IDbConnectionFactory db)
    {
        _db = db;
    }

    // ------------------------------------------------------------------
    // Officials
    // ------------------------------------------------------------------

    public async Task<IEnumerable<OfficialRow>> GetOfficialsAsync()
    {
        using var conn = _db.CreateConnection();
        const string sql = @"
            SELECT OfficialID, FullName, Role, IsCurrent, Email, Phone_No, Address
            FROM Ref_Officials
            ORDER BY IsCurrent DESC, FullName;";
        return await conn.QueryAsync<OfficialRow>(sql);
    }

    public async Task<string> CreateOfficialAsync(OfficialInputModel input)
    {
        using var conn = _db.CreateConnection();

        // OfficialID is server-generated (DEFAULT expression using
        // OfficialSeq), so OUTPUT INSERTED.OfficialID hands the new
        // ID straight back rather than re-querying for it.
        const string sql = @"
            INSERT INTO Ref_Officials (FullName, Role, IsCurrent, Email, Phone_No, Address)
            OUTPUT INSERTED.OfficialID
            VALUES (@FullName, @Role, @IsCurrent, @Email, @Phone_No, @Address);";

        return await conn.ExecuteScalarAsync<string>(sql, input);
    }

    public async Task<OfficialInputModel?> GetOfficialForEditAsync(string officialId)
    {
        using var conn = _db.CreateConnection();
        const string sql = @"
            SELECT FullName, Role, IsCurrent, Email, Phone_No, Address
            FROM Ref_Officials WHERE OfficialID = @OfficialId;";
        return await conn.QuerySingleOrDefaultAsync<OfficialInputModel>(sql, new { OfficialId = officialId });
    }

    public async Task UpdateOfficialAsync(string officialId, OfficialInputModel input)
    {
        using var conn = _db.CreateConnection();
        const string sql = @"
            UPDATE Ref_Officials
            SET FullName = @FullName, Role = @Role, IsCurrent = @IsCurrent,
                Email = @Email, Phone_No = @Phone_No, Address = @Address
            WHERE OfficialID = @OfficialId;";
        await conn.ExecuteAsync(sql, new
        {
            OfficialId = officialId,
            input.FullName,
            input.Role,
            input.IsCurrent,
            input.Email,
            input.Phone_No,
            input.Address
        });
    }

    // ------------------------------------------------------------------
    // Suppliers
    // ------------------------------------------------------------------

    public async Task<IEnumerable<SupplierRow>> GetSuppliersAsync()
    {
        using var conn = _db.CreateConnection();
        const string sql = @"
            SELECT Supplier_ID, Business_Name, Service_Category, Tax_PIN, Contact_Phone, Email, Address
            FROM Ref_Suppliers
            ORDER BY Business_Name;";
        return await conn.QueryAsync<SupplierRow>(sql);
    }

    public async Task<string> CreateSupplierAsync(SupplierInputModel input)
    {
        using var conn = _db.CreateConnection();

        // Supplier_ID is server-generated (DEFAULT expression using
        // SupplierSeq), same pattern as OfficialID above.
        const string sql = @"
            INSERT INTO Ref_Suppliers (Business_Name, Service_Category, Tax_PIN, Contact_Phone, Email, Address)
            OUTPUT INSERTED.Supplier_ID
            VALUES (@Business_Name, @Service_Category, @Tax_PIN, @Contact_Phone, @Email, @Address);";

        return await conn.ExecuteScalarAsync<string>(sql, input);
    }

    public async Task<SupplierInputModel?> GetSupplierForEditAsync(string supplierId)
    {
        using var conn = _db.CreateConnection();
        const string sql = @"
            SELECT Business_Name, Service_Category, Tax_PIN, Contact_Phone, Email, Address
            FROM Ref_Suppliers WHERE Supplier_ID = @SupplierId;";
        return await conn.QuerySingleOrDefaultAsync<SupplierInputModel>(sql, new { SupplierId = supplierId });
    }

    public async Task UpdateSupplierAsync(string supplierId, SupplierInputModel input)
    {
        using var conn = _db.CreateConnection();
        const string sql = @"
            UPDATE Ref_Suppliers
            SET Business_Name = @Business_Name, Service_Category = @Service_Category, Tax_PIN = @Tax_PIN,
                Contact_Phone = @Contact_Phone, Email = @Email, Address = @Address
            WHERE Supplier_ID = @SupplierId;";
        await conn.ExecuteAsync(sql, new
        {
            SupplierId = supplierId,
            input.Business_Name,
            input.Service_Category,
            input.Tax_PIN,
            input.Contact_Phone,
            input.Email,
            input.Address
        });
    }

    // ------------------------------------------------------------------
    // Budget Codes
    // ------------------------------------------------------------------

    public async Task<IEnumerable<BudgetCodeRow>> GetBudgetCodeRowsAsync()
    {
        using var conn = _db.CreateConnection();
        const string sql = @"
            SELECT Budget_ID, Category_Name, Category_Type
            FROM Ref_BudgetCodes
            ORDER BY Category_Name;";
        return await conn.QueryAsync<BudgetCodeRow>(sql);
    }

    public async Task<string> CreateBudgetCodeAsync(BudgetCodeInputModel input)
    {
        using var conn = _db.CreateConnection();

        // Budget_ID is server-generated (DEFAULT expression using
        // BudgetSeq), same pattern as OfficialID/Supplier_ID above.
        const string sql = @"
            INSERT INTO Ref_BudgetCodes (Category_Name, Category_Type)
            OUTPUT INSERTED.Budget_ID
            VALUES (@Category_Name, @Category_Type);";

        return await conn.ExecuteScalarAsync<string>(sql, input);
    }

    public async Task<BudgetCodeInputModel?> GetBudgetCodeForEditAsync(string budgetId)
    {
        using var conn = _db.CreateConnection();
        const string sql = @"
            SELECT Category_Name, Category_Type
            FROM Ref_BudgetCodes WHERE Budget_ID = @BudgetId;";
        return await conn.QuerySingleOrDefaultAsync<BudgetCodeInputModel>(sql, new { BudgetId = budgetId });
    }

    public async Task UpdateBudgetCodeAsync(string budgetId, BudgetCodeInputModel input)
    {
        using var conn = _db.CreateConnection();
        const string sql = @"
            UPDATE Ref_BudgetCodes
            SET Category_Name = @Category_Name, Category_Type = @Category_Type
            WHERE Budget_ID = @BudgetId;";
        await conn.ExecuteAsync(sql, new
        {
            BudgetId = budgetId,
            input.Category_Name,
            input.Category_Type
        });
    }
}
