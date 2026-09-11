using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UASU_VoucherApprovals.Models;
using UASU_VoucherApprovals.Services;
using UASU_VoucherApprovals.Utilities;

namespace UASU_VoucherApprovals.Pages.Reports;

// [Authorize] only, no role restriction - open to anyone with a login
// (Chairman, Chapter Secretary, Treasurer, etc.), per your choice.
[Authorize]
public class IndexModel : PageModel
{
    private readonly IReportService _reportService;

    public IndexModel(IReportService reportService)
    {
        _reportService = reportService;
    }

    [BindProperty(SupportsGet = true, Name = "period")]
    public ReportPeriod SelectedPeriod { get; set; } = ReportPeriod.Monthly;

    [BindProperty(SupportsGet = true, Name = "year")]
    public int? SelectedYear { get; set; }

    public IEnumerable<PeriodSummary> Rows { get; set; } = Enumerable.Empty<PeriodSummary>();
    public IEnumerable<BudgetBreakdownRow> BudgetBreakdown { get; set; } = Enumerable.Empty<BudgetBreakdownRow>();
    public IEnumerable<int> AvailableYears { get; set; } = Enumerable.Empty<int>();

    public record BudgetLineAmount(string Category_Name, decimal Amount);

    // Hover breakdown for the Income/Expense amounts on the main
    // summary table, derived entirely from BudgetBreakdown (already
    // fetched for the "View Details by Budget Code" table below) - no
    // extra query needed. Keyed by PeriodLabel for one period's cell;
    // the *Total lists are the same lines re-aggregated across every
    // period currently shown, for the Total row's cells. A category
    // with nothing of that type that period is excluded rather than
    // listed at zero.
    public Dictionary<string, List<BudgetLineAmount>> IncomeBreakdownByPeriod { get; set; } = new();
    public Dictionary<string, List<BudgetLineAmount>> ExpenseBreakdownByPeriod { get; set; } = new();
    public List<BudgetLineAmount> IncomeBreakdownTotal { get; set; } = new();
    public List<BudgetLineAmount> ExpenseBreakdownTotal { get; set; } = new();

    public async Task OnGetAsync()
    {
        AvailableYears = await _reportService.GetAvailableYearsAsync();

        // Default to the current year on first visit (no ?year= at all)
        // rather than dumping every year of history in one table; an
        // explicit "All years" selection (empty string) stays empty.
        if (SelectedYear is null && !Request.Query.ContainsKey("year"))
            SelectedYear = DateTime.Today.Year;

        Rows = await _reportService.GetSummaryAsync(SelectedPeriod, SelectedYear);
        BudgetBreakdown = (await _reportService.GetBudgetBreakdownAsync(SelectedPeriod, SelectedYear)).ToList();
        BuildHoverBreakdowns();
    }

    private void BuildHoverBreakdowns()
    {
        IncomeBreakdownByPeriod = BudgetBreakdown
            .Where(b => b.TotalIncome > 0)
            .GroupBy(b => b.PeriodLabel)
            .ToDictionary(
                g => g.Key,
                g => g.Select(b => new BudgetLineAmount(b.Category_Name, b.TotalIncome)).OrderByDescending(l => l.Amount).ToList());

        ExpenseBreakdownByPeriod = BudgetBreakdown
            .Where(b => b.TotalExpense > 0)
            .GroupBy(b => b.PeriodLabel)
            .ToDictionary(
                g => g.Key,
                g => g.Select(b => new BudgetLineAmount(b.Category_Name, b.TotalExpense)).OrderByDescending(l => l.Amount).ToList());

        IncomeBreakdownTotal = BudgetBreakdown
            .Where(b => b.TotalIncome > 0)
            .GroupBy(b => b.Category_Name)
            .Select(g => new BudgetLineAmount(g.Key, g.Sum(b => b.TotalIncome)))
            .OrderByDescending(l => l.Amount)
            .ToList();

        ExpenseBreakdownTotal = BudgetBreakdown
            .Where(b => b.TotalExpense > 0)
            .GroupBy(b => b.Category_Name)
            .Select(g => new BudgetLineAmount(g.Key, g.Sum(b => b.TotalExpense)))
            .OrderByDescending(l => l.Amount)
            .ToList();
    }

    // Reuses the same service call OnGetAsync uses, with whatever
    // period/year are on the URL - the export can never drift out of
    // sync with what's on screen.
    public async Task<IActionResult> OnGetExportSummaryCsvAsync()
    {
        var rows = await _reportService.GetSummaryAsync(SelectedPeriod, SelectedYear);
        var csv = CsvExport.Build(
            new[] { "Period", "Income", "Expense", "Net" },
            rows.Select(r => new object?[] { r.PeriodLabel, r.TotalIncome, r.TotalExpense, r.NetBalance }));

        return File(csv, "text/csv", $"IncomeExpense_{SelectedPeriod}_{SelectedYear?.ToString() ?? "AllYears"}.csv");
    }

    public async Task<IActionResult> OnGetExportBudgetCsvAsync()
    {
        var rows = await _reportService.GetBudgetBreakdownAsync(SelectedPeriod, SelectedYear);
        var csv = CsvExport.Build(
            new[] { "Period", "Budget Category", "Type", "Income", "Expense", "Net" },
            rows.Select(b => new object?[] { b.PeriodLabel, b.Category_Name, b.Category_Type, b.TotalIncome, b.TotalExpense, b.NetBalance }));

        return File(csv, "text/csv", $"BudgetBreakdown_{SelectedPeriod}_{SelectedYear?.ToString() ?? "AllYears"}.csv");
    }
}
