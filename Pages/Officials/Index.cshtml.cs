using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UASU_VoucherApprovals.Models;
using UASU_VoucherApprovals.Services;

namespace UASU_VoucherApprovals.Pages.Officials;

// Same gating as Add Logins / New Voucher / New Debt - this is
// reference data that feeds payee dropdowns and (for the four officer
// roles) login seeding, not something Approvers need to touch.
[Authorize(Policy = "TreasuryAdmin")]
public class IndexModel : PageModel
{
    private readonly IReferenceDataService _referenceDataService;

    public IndexModel(IReferenceDataService referenceDataService)
    {
        _referenceDataService = referenceDataService;
    }

    [BindProperty]
    public OfficialInputModel Input { get; set; } = new();

    public IEnumerable<OfficialRow> Officials { get; set; } = Enumerable.Empty<OfficialRow>();

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task OnGetAsync()
    {
        Officials = await _referenceDataService.GetOfficialsAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            Officials = await _referenceDataService.GetOfficialsAsync();
            return Page();
        }

        var officialId = await _referenceDataService.CreateOfficialAsync(Input);
        StatusMessage = $"Added {Input.FullName} ({officialId}).";

        // Redirect (PRG) so a refresh doesn't try to create a duplicate.
        return RedirectToPage();
    }
}
