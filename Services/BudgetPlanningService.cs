using Dapper;
using UASU_VoucherApprovals.Data;
using UASU_VoucherApprovals.Models;

namespace UASU_VoucherApprovals.Services;

// Proposed_Budget already exists in the database (migrated by SQL/004)
// but had no app-layer code at all until now - this is the first
// service to read or write it. Budget performance compares it against
// IncomeExpenseByYearAndBudget (SQL/007), which already aggregates
// actual payment activity per budget code per calendar year, so no new
// view is needed for the comparison itself.
public interface IBudgetPlanningService
{
    Task<IEnumerable<SimpleOption>> GetBudgetCodeOptionsAsync();
    Task<IEnumerable<int>> GetAvailableFiscalYearsAsync();

    Task<IEnumerable<ProposedBudgetRow>> GetProposedBudgetsAsync(int fiscalYear);
    Task<int?> CreateProposedBudgetAsync(ProposedBudgetInputModel input);
    Task<ProposedBudgetInputModel?> GetProposedBudgetForEditAsync(int proposalId);
    Task UpdateProposedBudgetAsync(int proposalId, ProposedBudgetInputModel input);

    Task<IEnumerable<BudgetPerformanceRow>> GetBudgetPerformanceAsync(int fiscalYear);
}

public class BudgetPlanningService : IBudgetPlanningService
{
    private readonly IDbConnectionFactory _db;

    public BudgetPlanningService(IDbConnectionFactory db)
    {
        _db = db;
    }

    public async Task<IEnumerable<SimpleOption>> GetBudgetCodeOptionsAsync()
    {
        using var conn = _db.CreateConnection();
        const string sql = @"
            SELECT Budget_ID AS Id, Category_Name + ' (' + Category_Type + ')' AS Label
            FROM Ref_BudgetCodes
            ORDER BY Category_Name;";
        return await conn.QueryAsync<SimpleOption>(sql);
    }

    // Years with either a proposed budget or actual activity, so the
    // year picker on both the Proposed Budgets and Budget Performance
    // pages covers every year worth looking at, not just planned ones.
    public async Task<IEnumerable<int>> GetAvailableFiscalYearsAsync()
    {
        using var conn = _db.CreateConnection();
        const string sql = @"
            SELECT DISTINCT Year FROM (
                SELECT Fiscal_Year AS Year FROM Proposed_Budget
                UNION
                SELECT Year FROM IncomeExpenseByYear
            ) x
            ORDER BY Year DESC;";
        return await conn.QueryAsync<int>(sql);
    }

    public async Task<IEnumerable<ProposedBudgetRow>> GetProposedBudgetsAsync(int fiscalYear)
    {
        using var conn = _db.CreateConnection();
        const string sql = @"
            SELECT pb.Proposal_ID, pb.Budget_Link, bc.Category_Name, bc.Category_Type,
                   pb.Fiscal_Year, pb.Proposed_Amount, pb.Notes, pb.Date_Added
            FROM Proposed_Budget pb
            INNER JOIN Ref_BudgetCodes bc ON bc.Budget_ID = pb.Budget_Link
            WHERE pb.Fiscal_Year = @FiscalYear
            ORDER BY bc.Category_Name;";
        return await conn.QueryAsync<ProposedBudgetRow>(sql, new { FiscalYear = fiscalYear });
    }

    // Returns null if a proposal already exists for this budget code and
    // fiscal year - the page turns that into a validation error pointing
    // at Edit instead, rather than silently creating a second row that
    // would double-count in GetBudgetPerformanceAsync's LEFT JOIN.
    public async Task<int?> CreateProposedBudgetAsync(ProposedBudgetInputModel input)
    {
        using var conn = _db.CreateConnection();

        const string existsSql = @"
            SELECT COUNT(*) FROM Proposed_Budget
            WHERE Budget_Link = @Budget_Link AND Fiscal_Year = @Fiscal_Year;";
        var alreadyExists = await conn.ExecuteScalarAsync<int>(existsSql, input) > 0;
        if (alreadyExists)
            return null;

        const string sql = @"
            INSERT INTO Proposed_Budget (Budget_Link, Fiscal_Year, Proposed_Amount, Notes, Date_Added)
            OUTPUT INSERTED.Proposal_ID
            VALUES (@Budget_Link, @Fiscal_Year, @Proposed_Amount, @Notes, GETDATE());";
        return await conn.ExecuteScalarAsync<int>(sql, input);
    }

    public async Task<ProposedBudgetInputModel?> GetProposedBudgetForEditAsync(int proposalId)
    {
        using var conn = _db.CreateConnection();
        const string sql = @"
            SELECT Budget_Link, Fiscal_Year, Proposed_Amount, Notes
            FROM Proposed_Budget WHERE Proposal_ID = @ProposalId;";
        return await conn.QuerySingleOrDefaultAsync<ProposedBudgetInputModel>(sql, new { ProposalId = proposalId });
    }

    public async Task UpdateProposedBudgetAsync(int proposalId, ProposedBudgetInputModel input)
    {
        using var conn = _db.CreateConnection();
        const string sql = @"
            UPDATE Proposed_Budget
            SET Budget_Link = @Budget_Link, Fiscal_Year = @Fiscal_Year,
                Proposed_Amount = @Proposed_Amount, Notes = @Notes
            WHERE Proposal_ID = @ProposalId;";
        await conn.ExecuteAsync(sql, new
        {
            ProposalId = proposalId,
            input.Budget_Link,
            input.Fiscal_Year,
            input.Proposed_Amount,
            input.Notes
        });
    }

    // Starts from every Ref_BudgetCodes row (so a code with no proposal
    // yet, or no activity yet, still shows up) and left-joins the
    // proposed amount for this year plus the actual totals already
    // computed by IncomeExpenseByYearAndBudget.
    public async Task<IEnumerable<BudgetPerformanceRow>> GetBudgetPerformanceAsync(int fiscalYear)
    {
        using var conn = _db.CreateConnection();
        const string sql = @"
            SELECT bc.Budget_ID, bc.Category_Name, bc.Category_Type,
                   pb.Proposed_Amount,
                   ISNULL(act.TotalIncome, 0) AS TotalIncome,
                   ISNULL(act.TotalExpense, 0) AS TotalExpense
            FROM Ref_BudgetCodes bc
            LEFT JOIN Proposed_Budget pb ON pb.Budget_Link = bc.Budget_ID AND pb.Fiscal_Year = @FiscalYear
            LEFT JOIN IncomeExpenseByYearAndBudget act ON act.Budget_ID = bc.Budget_ID AND act.Year = @FiscalYear
            ORDER BY bc.Category_Name;";
        return await conn.QueryAsync<BudgetPerformanceRow>(sql, new { FiscalYear = fiscalYear });
    }
}
