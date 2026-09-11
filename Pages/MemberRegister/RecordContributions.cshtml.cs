using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UASU_VoucherApprovals.Models;
using UASU_VoucherApprovals.Services;

namespace UASU_VoucherApprovals.Pages.MemberRegister;

// One page, one (Year, Month) at a time, every contributor as a row -
// mirrors how the underlying data actually arrives in practice (MMU
// payroll sends one checkoff deduction list per month covering
// everyone), rather than entering a whole year for one person at a time.
[Authorize(Policy = "TreasuryAdmin")]
public class RecordContributionsModel : PageModel
{
    private readonly IMemberRegisterService _memberRegisterService;

    public RecordContributionsModel(IMemberRegisterService memberRegisterService)
    {
        _memberRegisterService = memberRegisterService;
    }

    [BindProperty(SupportsGet = true, Name = "year")]
    public int SelectedYear { get; set; }

    [BindProperty(SupportsGet = true, Name = "month")]
    public int SelectedMonth { get; set; }

    [BindProperty]
    public List<ContributionEntryInput> Entries { get; set; } = new();

    public List<ContributionEntryRow> Grid { get; set; } = new();

    [TempData]
    public string? StatusMessage { get; set; }

    public static readonly string[] MonthNames =
        { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };

    public async Task OnGetAsync()
    {
        var today = DateTime.Today;
        if (SelectedYear == 0) SelectedYear = today.Year;
        if (SelectedMonth == 0) SelectedMonth = today.Month;

        Grid = (await _memberRegisterService.GetContributionEntryGridAsync(SelectedYear, SelectedMonth)).ToList();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await _memberRegisterService.SaveContributionsAsync(SelectedYear, SelectedMonth, Entries);
        StatusMessage = $"Saved contributions for {MonthNames[SelectedMonth - 1]} {SelectedYear}.";
        return RedirectToPage(new { year = SelectedYear, month = SelectedMonth });
    }
}
