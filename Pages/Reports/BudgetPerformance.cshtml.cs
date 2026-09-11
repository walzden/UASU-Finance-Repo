using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UASU_VoucherApprovals.Models;
using UASU_VoucherApprovals.Services;
using UASU_VoucherApprovals.Utilities;

namespace UASU_VoucherApprovals.Pages.Reports;

// [Authorize] only, no role restriction - same as Reports/Index, open to
// anyone with a login so Chairman/Chapter Secretary can see how the
// budget Treasury proposed is tracking without needing TreasuryAdmin.
[Authorize]
public class BudgetPerformanceModel : PageModel
{
    private readonly IBudgetPlanningService _budgetPlanningService;

    public BudgetPerformanceModel(IBudgetPlanningService budgetPlanningService)
    {
        _budgetPlanningService = budgetPlanningService;
    }

    [BindProperty(SupportsGet = true, Name = "year")]
    public int FiscalYear { get; set; }

    public IEnumerable<BudgetPerformanceRow> Rows { get; set; } = Enumerable.Empty<BudgetPerformanceRow>();
    public IEnumerable<int> AvailableYears { get; set; } = Enumerable.Empty<int>();

    public async Task OnGetAsync()
    {
        AvailableYears = await _budgetPlanningService.GetAvailableFiscalYearsAsync();

        // Default to the current year on first visit (no ?year= at all),
        // same convention as Reports/Index's SelectedYear default.
        if (!Request.Query.ContainsKey("year"))
            FiscalYear = DateTime.Today.Year;

        Rows = (await _budgetPlanningService.GetBudgetPerformanceAsync(FiscalYear))
            .OrderByDescending(r => r.Proposed_Amount.HasValue)
            .ThenBy(r => r.Category_Name)
            .ToList();
    }

    public async Task<IActionResult> OnGetExportCsvAsync()
    {
        var rows = await _budgetPlanningService.GetBudgetPerformanceAsync(FiscalYear);
        var csv = CsvExport.Build(
            new[] { "Budget Code", "Category", "Type", "Proposed", "Actual", "Variance", "% Used" },
            rows.Select(r => new object?[]
            {
                r.Budget_ID, r.Category_Name, r.Category_Type,
                r.Proposed_Amount, r.Actual, r.Variance,
                r.PercentUsed.HasValue ? r.PercentUsed.Value.ToString("F1") : null
            }));

        return File(csv, "text/csv", $"BudgetPerformance_{FiscalYear}.csv");
    }
}
