using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using UASU_VoucherApprovals.Models;
using UASU_VoucherApprovals.Services;

namespace UASU_VoucherApprovals.Pages.Honoraria;

[Authorize(Policy = "TreasuryAdmin")]
public class GenerateModel : PageModel
{
    private readonly IHonorariaService _honorariaService;
    private readonly IVoucherService _voucherService;

    public GenerateModel(IHonorariaService honorariaService, IVoucherService voucherService)
    {
        _honorariaService = honorariaService;
        _voucherService = voucherService;
    }

    [BindProperty(SupportsGet = true)]
    public int Year { get; set; } = DateTime.Today.Year;

    [BindProperty(SupportsGet = true)]
    public int Month { get; set; } = DateTime.Today.Month;

    // Blank means "use the standard 'October 2026 Monthly honoraria'".
    [BindProperty(SupportsGet = true)]
    public string? CustomDescription { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Budget_Link { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateTime? VoucherDate { get; set; }

    public IEnumerable<SimpleOption> BudgetCodes { get; set; } = Enumerable.Empty<SimpleOption>();
    public List<HonorariaPreviewRow> Preview { get; set; } = new();

    public string EffectiveDescription =>
        string.IsNullOrWhiteSpace(CustomDescription)
            ? $"{new DateTime(Year, Month, 1).ToString("MMMM yyyy", CultureInfo.InvariantCulture)} Monthly honoraria"
            : CustomDescription.Trim();

    public int ReadyCount => Preview.Count(p => p.Status == "Ready");
    public decimal ReadyTotal => Preview.Where(p => p.Status == "Ready").Sum(p => p.Amount ?? 0);

    [TempData]
    public string? StatusMessage { get; set; }

    public bool StatusIsError { get; set; }

    public async Task OnGetAsync()
    {
        await LoadAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await LoadAsync();

        if (string.IsNullOrWhiteSpace(Budget_Link))
        {
            StatusIsError = true;
            StatusMessage = "Choose a budget code for these vouchers.";
            return Page();
        }

        if (ReadyCount == 0)
        {
            StatusIsError = true;
            StatusMessage = "There are no vouchers left to generate for this description.";
            return Page();
        }

        try
        {
            var ids = await _honorariaService.GenerateAsync(
                EffectiveDescription, VoucherDate ?? DateTime.Today, Budget_Link);
            StatusMessage =
                $"Created {ids.Count} voucher(s) for \"{EffectiveDescription}\" ({ids.First()} to {ids.Last()}). They now await approval under Pending Vouchers.";
            return RedirectToPage(new
            {
                Month,
                Year,
                CustomDescription,
                Budget_Link,
                VoucherDate = (VoucherDate ?? DateTime.Today).ToString("yyyy-MM-dd")
            });
        }
        catch (SqlException ex)
        {
            StatusIsError = true;
            StatusMessage = ex.Message;
            return Page();
        }
    }

    private async Task LoadAsync()
    {
        if (Month < 1 || Month > 12) Month = DateTime.Today.Month;
        if (Year < 2000 || Year > 2100) Year = DateTime.Today.Year;

        BudgetCodes = (await _voucherService.GetBudgetCodesAsync())
            .Where(b => b.Label.EndsWith("(Expense)", StringComparison.OrdinalIgnoreCase))
            .ToList();

        Budget_Link ??= BudgetCodes
            .FirstOrDefault(b => b.Label.Contains("honorar", StringComparison.OrdinalIgnoreCase))?.Id;

        Preview = (await _honorariaService.GetPreviewAsync(EffectiveDescription)).ToList();
    }
}
