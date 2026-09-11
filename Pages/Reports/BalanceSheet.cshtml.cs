using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UASU_VoucherApprovals.Models;
using UASU_VoucherApprovals.Services;

namespace UASU_VoucherApprovals.Pages.Reports;

// [Authorize] only, read-only - same as every other report. A
// "Statement of Financial Position" as at a chosen month-end, meant to
// make end-of-year reporting easier - pick December of the fiscal year
// and this gives Assets/Liabilities/Net Assets straight from the
// figures already trusted for monthly certification and debt tracking,
// rather than someone re-deriving them by hand from several reports.
[Authorize]
public class BalanceSheetModel : PageModel
{
    private readonly IReportService _reportService;

    public BalanceSheetModel(IReportService reportService)
    {
        _reportService = reportService;
    }

    [BindProperty(SupportsGet = true, Name = "year")]
    public int SelectedYear { get; set; }

    [BindProperty(SupportsGet = true, Name = "month")]
    public int SelectedMonth { get; set; }

    public BalanceSheetData Data { get; set; } = new();

    // Supporting detail behind the headline Cash/Bank figures - Income
    // and Expense broken down by budget code for the SELECTED YEAR
    // (a flow/movement figure), not the same thing as the Balance
    // Sheet's cumulative-all-time Assets/Liabilities/Net Assets above
    // it, so it isn't expected to reconcile to Net Assets - it's the
    // existing Budget Performance breakdown (IncomeExpenseByYearAndBudget,
    // Yearly period), reused as-is rather than recomputed.
    public List<BudgetBreakdownRow> BudgetBreakdown { get; set; } = new();

    public async Task OnGetAsync()
    {
        var today = DateTime.Today;
        if (SelectedYear == 0) SelectedYear = today.Year;
        if (SelectedMonth == 0) SelectedMonth = today.Month;

        Data = await _reportService.GetBalanceSheetAsync(SelectedYear, SelectedMonth);
        BudgetBreakdown = (await _reportService.GetBudgetBreakdownAsync(ReportPeriod.Yearly, SelectedYear)).ToList();
    }
}
