using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UASU_VoucherApprovals.Models;
using UASU_VoucherApprovals.Services;
using UASU_VoucherApprovals.Utilities;

namespace UASU_VoucherApprovals.Pages.Certification;

[Authorize(Policy = "TreasuryAdmin")]
public class IndexModel : PageModel
{
    private readonly ICertificationService _certificationService;
    private readonly ICashFloatService _floatService;

    public IndexModel(ICertificationService certificationService, ICashFloatService floatService)
    {
        _certificationService = certificationService;
        _floatService = floatService;
    }

    [BindProperty(SupportsGet = true)]
    public int Year { get; set; }

    [BindProperty(SupportsGet = true)]
    public int Month { get; set; }

    [BindProperty]
    public CertificationInputModel Input { get; set; } = new();

    [BindProperty]
    public OpeningBalanceInputModel OpeningBalanceInput { get; set; } = new();

    public decimal BookCashBalance { get; set; }
    public decimal BookBankBalance { get; set; }

    // Cash floats open at month-end: part of BookCashBalance is out with
    // their custodians rather than in the treasury's own hands.
    public IEnumerable<CashFloatRow> Floats { get; set; } = Enumerable.Empty<CashFloatRow>();
    public decimal CashInFloats => Floats.Sum(f => f.BookOutstanding);
    public decimal FloatCashWithCustodians => Floats.Sum(f => f.CashWithCustodian);
    public decimal FloatReceiptsNotSettled => Floats.Sum(f => f.Spent - f.Settled);
    public IEnumerable<CertificationRow> Certifications { get; set; } = Enumerable.Empty<CertificationRow>();

    // ---- Preview before certifying ----

    // Book balances at the end of the previous month, so the page can show
    // the month's net movement (opening -> closing) for cash and bank.
    public decimal OpeningCash { get; set; }
    public decimal OpeningBank { get; set; }

    public List<VarianceHint> Hints { get; set; } = new();

    // Set by "Check figures": the actual figures were compared with the
    // book and nothing was saved. Certify is offered only after this.
    public bool Checked { get; set; }
    public decimal CashVariance => Input.Actual_Cash_On_Hand - BookCashBalance;
    public decimal BankVariance => Input.Actual_Bank_Balance - BookBankBalance;
    public bool Agrees => CashVariance == 0 && BankVariance == 0;

    // The loaded month's existing certification, if any - it can't be
    // certified again (UX_MonthlyCertifications_Period, SQL/037).
    public CertificationRow? ExistingCertification { get; set; }

    // Book balances as they compute today for each certified month. When
    // they differ from what was certified, something dated in that month
    // was changed afterwards - the history flags it.
    public Dictionary<string, (decimal Cash, decimal Bank)> CurrentBook { get; set; } = new();

    public string? ErrorMessage { get; set; }

    [TempData]
    public string? StatusMessage { get; set; }

    private string CurrentOfficialId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    public async Task OnGetAsync()
    {
        Certifications = await _certificationService.GetCertificationsAsync();
        OpeningBalanceInput = await _certificationService.GetOpeningBalanceAsync();

        if (Year == 0 || Month == 0)
        {
            // Default to the month right after whichever period was most
            // recently certified, falling back to the opening balance's
            // own month if nothing has been certified yet - NOT "last
            // calendar month". Certification usually proceeds in order
            // from an opening balance forward, and "last calendar month"
            // can land on a period the user never intended to touch -
            // this has already produced one certification recorded
            // against the wrong month because the page defaulted
            // somewhere other than where the user was actually working.
            var latest = Certifications
                .OrderByDescending(c => c.Period_Year)
                .ThenByDescending(c => c.Period_Month)
                .FirstOrDefault();

            var defaultPeriod = latest is not null
                ? new DateTime(latest.Period_Year, latest.Period_Month, 1).AddMonths(1)
                : new DateTime(OpeningBalanceInput.As_Of_Date.Year, OpeningBalanceInput.As_Of_Date.Month, 1);

            Year = defaultPeriod.Year;
            Month = defaultPeriod.Month;
        }

        Input.Period_Year = Year;
        Input.Period_Month = Month;

        await LoadPageAsync();
    }

    // "Check figures": compare the actual cash and bank with the book and
    // list what could explain any difference. Saves nothing.
    public async Task<IActionResult> OnPostCheckAsync()
    {
        ModelState.Clear();
        Year = Input.Period_Year;
        Month = Input.Period_Month;

        if (RequireAmountsEntered() && TryValidateModel(Input, nameof(Input)))
            Checked = true;

        await LoadPageAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // Two separate forms share this page (Certify / Set Opening
        // Balance) - only re-validate the one that was actually
        // submitted, otherwise the other form's untouched, still-default
        // fields (e.g. Period_Month = 0) fail their own validation.
        ModelState.Clear();
        Year = Input.Period_Year;
        Month = Input.Period_Month;

        if (!RequireAmountsEntered() || !TryValidateModel(Input, nameof(Input)))
        {
            await LoadPageAsync();
            return Page();
        }

        await LoadPageAsync();

        if (ExistingCertification is not null)
        {
            ErrorMessage = $"{MonthName(Month)} {Year} was already certified on {ExistingCertification.Certified_Date:d MMM yyyy} ({ExistingCertification.Certification_ID}). A month can only be certified once.";
            return Page();
        }

        // The page only offers Certify after "Check figures", but the rule
        // that matters is enforced here: a month that doesn't agree must
        // say why.
        Checked = true;
        if (!Agrees && string.IsNullOrWhiteSpace(Input.Notes))
        {
            ModelState.AddModelError("Input.Notes",
                "The figures don't agree with the book. Explain the variance in the notes before certifying.");
            return Page();
        }

        string certificationId;
        try
        {
            certificationId = await _certificationService.CertifyAsync(Input, CurrentOfficialId);
        }
        catch (Microsoft.Data.SqlClient.SqlException ex)
        {
            // e.g. UX_MonthlyCertifications_Period if someone else certified
            // this month in the meantime.
            ErrorMessage = ex.Number is 2601 or 2627
                ? $"{MonthName(Month)} {Year} has already been certified. A month can only be certified once."
                : ex.Message;
            return Page();
        }
        StatusMessage = $"Certified {Input.Period_Year}-{Input.Period_Month:D2} ({certificationId}).";

        // Redirect (PRG) back to the same period so a refresh doesn't
        // try to certify the same month again.
        return RedirectToPage(new { year = Input.Period_Year, month = Input.Period_Month });
    }

    public async Task<IActionResult> OnPostSetOpeningBalanceAsync()
    {
        ModelState.Clear();

        if (!TryValidateModel(OpeningBalanceInput, nameof(OpeningBalanceInput)))
        {
            var typed = OpeningBalanceInput;   // keep what was typed
            await LoadPageAsync();
            OpeningBalanceInput = typed;
            return Page();
        }

        await _certificationService.SetOpeningBalanceAsync(OpeningBalanceInput, CurrentOfficialId);
        StatusMessage = "Opening balance updated.";

        return RedirectToPage(new { year = Year, month = Month });
    }

    private async Task LoadBalancesAsync(int year, int month)
    {
        var monthStart = new DateTime(year, month, 1);
        var monthEnd = monthStart.AddMonths(1).AddDays(-1);

        (BookCashBalance, BookBankBalance) = await _certificationService.GetBookBalancesAsync(year, month);
        (OpeningCash, OpeningBank) = await _certificationService.GetBookBalancesAsOfAsync(monthStart.AddDays(-1));
        Floats = await _floatService.GetPositionsAsOfAsync(monthEnd);
        Hints = await _certificationService.GetVarianceHintsAsync(monthEnd);
    }

    // Everything the page shows for the loaded month, used by every handler.
    private async Task LoadPageAsync()
    {
        Certifications = (await _certificationService.GetCertificationsAsync()).ToList();
        OpeningBalanceInput = await _certificationService.GetOpeningBalanceAsync();
        ExistingCertification = Certifications.FirstOrDefault(c => c.Period_Year == Year && c.Period_Month == Month);
        await LoadBalancesAsync(Year, Month);

        foreach (var c in Certifications)
            CurrentBook[c.Certification_ID] = await _certificationService.GetBookBalancesAsync(c.Period_Year, c.Period_Month);
    }

    // ModelState.Clear() above also clears "the value '' is invalid", so a
    // blank amount would otherwise bind silently as 0 and be compared or
    // certified as Sh. 0. Both amounts must actually be entered.
    private bool RequireAmountsEntered()
    {
        var ok = true;
        foreach (var field in new[] { "Input.Actual_Cash_On_Hand", "Input.Actual_Bank_Balance" })
        {
            if (string.IsNullOrWhiteSpace(Request.Form[field]))
            {
                ModelState.AddModelError(field, "Enter the amount (0 if there is none).");
                ok = false;
            }
        }
        return ok;
    }

    public static string MonthName(int month) =>
        System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName(month);

    // Full certification history - already uncapped, unlike the
    // Acknowledge page's "recent 5" list, so this reuses it directly.
    public async Task<IActionResult> OnGetExportCsvAsync()
    {
        var rows = await _certificationService.GetCertificationsAsync();
        var csv = CsvExport.Build(
            new[] { "Period", "Book Cash", "Actual Cash", "Book Bank", "Actual Bank", "Agrees", "Certified By", "Certified Date", "Notes" },
            rows.Select(c => new object?[]
            {
                $"{c.Period_Year}-{c.Period_Month:D2}", c.Book_Cash_Balance, c.Actual_Cash_On_Hand,
                c.Book_Bank_Balance, c.Actual_Bank_Balance, c.Balances, c.CertifiedByName, c.Certified_Date, c.Notes
            }));

        return File(csv, "text/csv", "MonthlyCertifications.csv");
    }
}
