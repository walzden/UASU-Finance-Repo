using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UASU_VoucherApprovals.Models;
using UASU_VoucherApprovals.Services;
using UASU_VoucherApprovals.Utilities;

namespace UASU_VoucherApprovals.Pages.Reports;

// How finely to slice the debt list by Date_Incurred before splitting
// it into the Settled/Unpaid groups below.
public enum DebtPeriodType { All, Year, Quarter, Month }

[Authorize] // open to any signed-in user, same as the income/expense reports
public class DebtsModel : PageModel
{
    private readonly IDebtService _debtService;

    public DebtsModel(IDebtService debtService)
    {
        _debtService = debtService;
    }

    [BindProperty(SupportsGet = true, Name = "periodType")]
    public DebtPeriodType PeriodType { get; set; } = DebtPeriodType.All;

    [BindProperty(SupportsGet = true, Name = "period")]
    public string? Period { get; set; }

    // Options for the period-value dropdown, most recent first - empty
    // when PeriodType is All, since there's nothing to pick.
    public List<string> AvailablePeriods { get; set; } = new();

    public IEnumerable<DebtStatusRow> Rows { get; set; } = Enumerable.Empty<DebtStatusRow>();

    // The two groupings the report is built around. Partially Paid
    // debts count as Unpaid, not their own group - they still show
    // their own status badge in the table.
    public IEnumerable<DebtStatusRow> SettledRows => Rows.Where(r => r.SettlementStatus == "Settled");
    public IEnumerable<DebtStatusRow> UnpaidRows => Rows.Where(r => r.SettlementStatus != "Settled");

    public async Task OnGetAsync()
    {
        var all = (await _debtService.GetDebtStatusReportAsync()).ToList();
        AvailablePeriods = BuildAvailablePeriods(all, PeriodType);

        // No explicit ?period= yet (first visit after switching period
        // type) - default to the most recent slice rather than an
        // empty report.
        if (PeriodType != DebtPeriodType.All && Period is null && AvailablePeriods.Count > 0)
            Period = AvailablePeriods[0];

        Rows = Filter(all, PeriodType, Period);
    }

    // Reuses the same service call OnGetAsync uses, filtered the same
    // way, so the export can never drift out of sync with what's on
    // screen - it just can't be split into two worksheets, so a Group
    // column stands in for the on-screen Settled/Unpaid split.
    public async Task<IActionResult> OnGetExportCsvAsync()
    {
        var all = (await _debtService.GetDebtStatusReportAsync()).ToList();
        var filtered = Filter(all, PeriodType, Period)
            .OrderBy(d => d.SettlementStatus == "Settled" ? 0 : 1)
            .ThenBy(d => d.Date_Incurred);

        var csv = CsvExport.Build(
            new[] { "Group", "Debt", "Creditor", "Date Incurred", "Description", "Owed", "Paid", "Balance", "Status", "Aging" },
            filtered.Select(d => new object?[]
            {
                d.SettlementStatus == "Settled" ? "Settled" : "Unpaid",
                d.Debt_ID, d.CreditorName, d.Date_Incurred, d.Description,
                d.Total_Owed, d.TotalAllocated, d.Balance, d.SettlementStatus, d.AgingBucket
            }));

        return File(csv, "text/csv", "DebtStatus.csv");
    }

    // ---- period filtering helpers ----

    public static string PeriodKey(DebtStatusRow row, DebtPeriodType type) => type switch
    {
        DebtPeriodType.Year => $"{row.Date_Incurred.Year}",
        DebtPeriodType.Quarter => $"{row.Date_Incurred.Year}-Q{(row.Date_Incurred.Month - 1) / 3 + 1}",
        DebtPeriodType.Month => $"{row.Date_Incurred:yyyy-MM}",
        _ => "All"
    };

    public static string PeriodLabel(string key, DebtPeriodType type)
    {
        if (type == DebtPeriodType.Quarter)
        {
            var parts = key.Split("-Q");
            return $"Q{parts[1]} {parts[0]}";
        }
        if (type == DebtPeriodType.Month)
        {
            var parts = key.Split('-');
            var monthName = new DateTime(int.Parse(parts[0]), int.Parse(parts[1]), 1).ToString("MMMM");
            return $"{monthName} {parts[0]}";
        }
        return key;
    }

    private static List<string> BuildAvailablePeriods(List<DebtStatusRow> rows, DebtPeriodType type)
    {
        if (type == DebtPeriodType.All) return new List<string>();
        return rows.Select(r => PeriodKey(r, type)).Distinct().OrderByDescending(k => k).ToList();
    }

    private static List<DebtStatusRow> Filter(List<DebtStatusRow> rows, DebtPeriodType type, string? period)
    {
        if (type == DebtPeriodType.All || period is null) return rows;
        return rows.Where(r => PeriodKey(r, type) == period).ToList();
    }
}
