using System.ComponentModel.DataAnnotations;

namespace UASU_VoucherApprovals.Models;

// Ref_BudgetCodes has no CHECK constraint on Category_Type (confirmed
// against SQL/004's migration script, the only place this table's shape
// is written down) - free text with a datalist suggesting Income/Expense,
// same pattern as OfficialInputModel.Role.
public class BudgetCodeInputModel
{
    [Required]
    [MaxLength(150)]
    public string Category_Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Category_Type { get; set; } = "Expense";
}

public class BudgetCodeRow
{
    public string Budget_ID { get; set; } = string.Empty;
    public string Category_Name { get; set; } = string.Empty;
    public string Category_Type { get; set; } = string.Empty;
}

// Proposed_Budget already exists in the database (migrated by SQL/004)
// but had no app-layer model until now. Proposal_ID is a plain IDENTITY
// int - the one ID in this app that isn't a sequence-backed string.
public class ProposedBudgetInputModel
{
    [Required]
    public string Budget_Link { get; set; } = string.Empty;

    [Required]
    [Range(2000, 2100, ErrorMessage = "Enter a valid fiscal year.")]
    public int Fiscal_Year { get; set; } = DateTime.Today.Year;

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Proposed amount must be greater than zero.")]
    public decimal Proposed_Amount { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }
}

public class ProposedBudgetRow
{
    public int Proposal_ID { get; set; }
    public string Budget_Link { get; set; } = string.Empty;
    public string Category_Name { get; set; } = string.Empty;
    public string Category_Type { get; set; } = string.Empty;
    public int Fiscal_Year { get; set; }
    public decimal Proposed_Amount { get; set; }
    public string? Notes { get; set; }
    public DateTime Date_Added { get; set; }
}

// One row of the budget-vs-actual comparison for a single fiscal year -
// starts from every Ref_BudgetCodes row (so a code with no proposal yet,
// or no activity yet, still shows up) and left-joins the proposed amount
// plus the actual totals already computed by IncomeExpenseByYearAndBudget
// (SQL/007). Actual/Variance/PercentUsed are computed here rather than in
// SQL since they depend on Category_Type, which the view doesn't branch on.
public class BudgetPerformanceRow
{
    public string Budget_ID { get; set; } = string.Empty;
    public string Category_Name { get; set; } = string.Empty;
    public string Category_Type { get; set; } = string.Empty;
    public decimal? Proposed_Amount { get; set; }
    public decimal TotalIncome { get; set; }
    public decimal TotalExpense { get; set; }

    // Income categories are measured against money received, Expense
    // categories against money spent - matches Transaction_Type's own
    // Income/Expense split everywhere else in the app.
    public decimal Actual => string.Equals(Category_Type, "Income", StringComparison.OrdinalIgnoreCase)
        ? TotalIncome
        : TotalExpense;

    public decimal? Variance => Proposed_Amount.HasValue ? Proposed_Amount.Value - Actual : null;

    public decimal? PercentUsed => Proposed_Amount is > 0 ? Actual / Proposed_Amount.Value * 100 : null;
}
