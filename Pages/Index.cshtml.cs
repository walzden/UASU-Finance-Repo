using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UASU_VoucherApprovals.Models;
using UASU_VoucherApprovals.Services;

namespace UASU_VoucherApprovals.Pages;

// No [Authorize] here on purpose - anonymous visitors are redirected to
// the login page itself (below) rather than being shown a bare "Sign in"
// link, so "/" effectively IS the login page for anyone not signed in.
public class IndexModel : PageModel
{
    private readonly IReportService _reportService;
    private readonly IDebtService _debtService;
    private readonly ICertificationService _certificationService;
    private readonly IBudgetPlanningService _budgetPlanningService;

    public IndexModel(IReportService reportService, IDebtService debtService, ICertificationService certificationService, IBudgetPlanningService budgetPlanningService)
    {
        _reportService = reportService;
        _debtService = debtService;
        _certificationService = certificationService;
        _budgetPlanningService = budgetPlanningService;
    }

    public decimal YtdIncome { get; set; }
    public decimal YtdExpense { get; set; }
    public decimal NetBalance { get; set; }
    public decimal OutstandingDebt { get; set; }

    public List<PeriodSummary> MonthlyTrend { get; set; } = new();
    public List<BudgetBreakdownRow> TopExpenseCategories { get; set; } = new();

    // Only categories with a proposed amount for the current year - a
    // code with no proposal has nothing to compare against, so it's
    // excluded here rather than shown with a blank bar. Capped to the
    // largest proposals so the chart stays readable; the full set
    // (including no-proposal codes) is on Reports/BudgetPerformance.
    public List<BudgetPerformanceRow> BudgetPerformance { get; set; } = new();
    public int CategoriesOverBudget { get; set; }
    public int CategoriesWithProposal { get; set; }

    public decimal DebtAging0to30 { get; set; }
    public decimal DebtAging31to60 { get; set; }
    public decimal DebtAging61to90 { get; set; }
    public decimal DebtAging90Plus { get; set; }

    public string? LastCertifiedPeriod { get; set; }
    public bool CertificationOverdue { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        if (User.Identity?.IsAuthenticated != true)
            return RedirectToPage("/Account/Login");

        var year = DateTime.Today.Year;

        var yearly = await _reportService.GetSummaryAsync(ReportPeriod.Yearly, year);
        var currentYearTotals = yearly.FirstOrDefault();
        YtdIncome = currentYearTotals?.TotalIncome ?? 0;
        YtdExpense = currentYearTotals?.TotalExpense ?? 0;
        NetBalance = currentYearTotals?.NetBalance ?? 0;

        // GetSummaryAsync already comes back most-recent-first within the
        // year (DB-ordered by the real SortKey) - reversing that gives a
        // correct left-to-right chronological chart without needing to
        // re-sort on PeriodLabel, which isn't safely string-sortable
        // ("July 2026" < "June 2026" alphabetically, but not in time).
        var monthly = await _reportService.GetSummaryAsync(ReportPeriod.Monthly, year);
        MonthlyTrend = monthly.Reverse().ToList();

        var budget = await _reportService.GetBudgetBreakdownAsync(ReportPeriod.Yearly, year);
        TopExpenseCategories = budget
            .Where(b => b.Category_Type == "Expense" && b.TotalExpense > 0)
            .OrderByDescending(b => b.TotalExpense)
            .Take(5)
            .ToList();

        var performance = (await _budgetPlanningService.GetBudgetPerformanceAsync(year)).ToList();
        CategoriesOverBudget = performance.Count(r => r.Variance is < 0);
        CategoriesWithProposal = performance.Count(r => r.Proposed_Amount.HasValue);
        BudgetPerformance = performance
            .Where(r => r.Proposed_Amount.HasValue)
            .OrderByDescending(r => r.Proposed_Amount)
            .Take(8)
            .ToList();

        var debts = (await _debtService.GetDebtStatusReportAsync()).ToList();
        OutstandingDebt = debts.Sum(d => d.Balance);
        DebtAging0to30 = debts.Where(d => d.AgingBucket == "0-30 days").Sum(d => d.Balance);
        DebtAging31to60 = debts.Where(d => d.AgingBucket == "31-60 days").Sum(d => d.Balance);
        DebtAging61to90 = debts.Where(d => d.AgingBucket == "61-90 days").Sum(d => d.Balance);
        DebtAging90Plus = debts.Where(d => d.AgingBucket == "90+ days").Sum(d => d.Balance);

        var certifications = await _certificationService.GetCertificationsAsync();
        var latest = certifications
            .OrderByDescending(c => c.Period_Year)
            .ThenByDescending(c => c.Period_Month)
            .FirstOrDefault();

        if (latest is not null)
            LastCertifiedPeriod = $"{CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName(latest.Period_Month)} {latest.Period_Year}";

        // "Overdue" means the month that just ended has no certification
        // yet - matches Certification/Index's own default period.
        var expectedPeriod = DateTime.Today.AddMonths(-1);
        CertificationOverdue = latest is null
            || latest.Period_Year < expectedPeriod.Year
            || (latest.Period_Year == expectedPeriod.Year && latest.Period_Month < expectedPeriod.Month);

        return Page();
    }
}
