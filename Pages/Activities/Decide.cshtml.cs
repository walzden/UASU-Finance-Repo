using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using UASU_VoucherApprovals.Models;
using UASU_VoucherApprovals.Services;

namespace UASU_VoucherApprovals.Pages.Activities;

// TreasuryAdmin only (Treasurer / Deputy Treasurer). Nobody decides their
// own lines - trg_ActivityParticipants_ValidateDecision (SQL/030) enforces
// that and the treasury-role rule independently of this page.
[Authorize(Policy = "TreasuryAdmin")]
public class DecideModel : PageModel
{
    private readonly IActivityService _activityService;
    private readonly IVoucherService _voucherService;

    public DecideModel(IActivityService activityService, IVoucherService voucherService)
    {
        _activityService = activityService;
        _voucherService = voucherService;
    }

    // yyyy-MM
    [BindProperty(SupportsGet = true)]
    public string? Month { get; set; }

    [BindProperty]
    public List<LineDecisionInput> Lines { get; set; } = new();

    [BindProperty]
    public DateTime? VoucherDate { get; set; }

    public List<string> Months { get; set; } = new();
    public IReadOnlyList<OpenActivityLine> OpenLines { get; set; } = Array.Empty<OpenActivityLine>();
    public Dictionary<string, MonthCategoryCount> Counts { get; set; } = new();
    public List<SimpleOption> BudgetOptions { get; set; } = new();
    public Dictionary<string, string> BudgetLabels { get; set; } = new();

    public List<VoucherPlan> Plans { get; set; } = new();
    public List<(OpenActivityLine Line, LineDecisionInput Decision)> NotPayablePreview { get; set; } = new();
    public List<string> Errors { get; set; } = new();
    public bool ShowPreview { get; set; }

    [TempData]
    public string? StatusMessage { get; set; }

    public string CurrentOfficialId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    public DateTime MonthStart { get; private set; }

    public async Task OnGetAsync()
    {
        await LoadAsync();
    }

    public async Task<IActionResult> OnPostPreviewAsync()
    {
        await LoadAsync();

        var (pay, notPayable, errors) = _activityService.ValidateDecisions(Lines, OpenLines, BudgetLabels, CurrentOfficialId);
        Errors = errors;
        if (pay.Count == 0 && notPayable.Count == 0 && errors.Count == 0)
            Errors.Add("Decide at least one line first.");

        if (Errors.Count == 0)
        {
            Plans = _activityService.PlanVouchers(pay, BudgetLabels);
            NotPayablePreview = notPayable;
            ShowPreview = true;
        }

        return Page();
    }

    public async Task<IActionResult> OnPostCreateAsync()
    {
        await LoadAsync();

        try
        {
            var result = await _activityService.DecideAsync(Lines, CurrentOfficialId, VoucherDate ?? DateTime.Today, BudgetLabels);
            StatusMessage = result.VoucherIds.Count > 0
                ? $"Created {result.VoucherIds.Count} voucher(s) ({string.Join(", ", result.VoucherIds)}) and recorded {result.NotPayableCount} not-payable decision(s). The vouchers now wait for the Chairman and Chapter Secretary under Pending Approvals."
                : $"Recorded {result.NotPayableCount} not-payable decision(s).";
            return RedirectToPage(new { Month });
        }
        catch (InvalidOperationException ex)
        {
            Errors.Add(ex.Message);
        }
        catch (SqlException ex)
        {
            // A trigger (SQL/030) rejecting the decision, e.g. deciding your own line.
            Errors.Add(ex.Message);
        }

        return Page();
    }

    public LineDecisionInput PostedFor(string activityId, string officialId) =>
        Lines.FirstOrDefault(l => l.Activity_ID == activityId && l.OfficialID == officialId) ?? new LineDecisionInput();

    public string CountChips(string officialId) =>
        string.Join(" ", Counts.Values
            .Where(c => c.OfficialID == officialId && c.Total > 1)
            .OrderBy(c => c.Category)
            .Select(c => $"{c.Total} × {c.Category} this month" + (c.Paid > 0 ? $" ({c.Paid} already paid)" : "")));

    private async Task LoadAsync()
    {
        Months = (await _activityService.GetOpenMonthsAsync()).ToList();
        if (string.IsNullOrWhiteSpace(Month) || !DateTime.TryParseExact(Month + "-01", "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
            Month = Months.FirstOrDefault() ?? DateTime.Today.ToString("yyyy-MM");
        if (!Months.Contains(Month))
            Months.Insert(0, Month);

        MonthStart = DateTime.ParseExact(Month + "-01", "yyyy-MM-dd", CultureInfo.InvariantCulture);

        // Default the voucher date to the end of that month if it is past, otherwise today.
        VoucherDate ??= MonthStart.AddMonths(1).AddDays(-1) < DateTime.Today ? MonthStart.AddMonths(1).AddDays(-1) : DateTime.Today;

        BudgetOptions = (await _voucherService.GetBudgetCodesAsync())
            .Where(b => b.Label.EndsWith("(Expense)", StringComparison.OrdinalIgnoreCase))
            .ToList();
        BudgetLabels = BudgetOptions.ToDictionary(b => b.Id, b => b.Label);

        OpenLines = await _activityService.GetOpenLinesAsync(MonthStart);
        Counts = (await _activityService.GetMonthCountsAsync(MonthStart))
            .ToDictionary(c => c.OfficialID + "|" + c.Category);
    }
}
