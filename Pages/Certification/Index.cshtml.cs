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

    public IndexModel(ICertificationService certificationService)
    {
        _certificationService = certificationService;
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
    public IEnumerable<CertificationRow> Certifications { get; set; } = Enumerable.Empty<CertificationRow>();

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

        (BookCashBalance, BookBankBalance) = await _certificationService.GetBookBalancesAsync(Year, Month);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // Two separate forms share this page (Certify / Set Opening
        // Balance) - only re-validate the one that was actually
        // submitted, otherwise the other form's untouched, still-default
        // fields (e.g. Period_Month = 0) fail their own validation.
        ModelState.Clear();

        if (!TryValidateModel(Input, nameof(Input)))
        {
            (BookCashBalance, BookBankBalance) = await _certificationService.GetBookBalancesAsync(Input.Period_Year, Input.Period_Month);
            Certifications = await _certificationService.GetCertificationsAsync();
            OpeningBalanceInput = await _certificationService.GetOpeningBalanceAsync();
            return Page();
        }

        var certificationId = await _certificationService.CertifyAsync(Input, CurrentOfficialId);
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
            (BookCashBalance, BookBankBalance) = await _certificationService.GetBookBalancesAsync(Year, Month);
            Certifications = await _certificationService.GetCertificationsAsync();
            return Page();
        }

        await _certificationService.SetOpeningBalanceAsync(OpeningBalanceInput, CurrentOfficialId);
        StatusMessage = "Opening balance updated.";

        return RedirectToPage(new { year = Year, month = Month });
    }

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
