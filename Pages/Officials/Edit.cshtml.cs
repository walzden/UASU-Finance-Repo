using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UASU_VoucherApprovals.Models;
using UASU_VoucherApprovals.Services;

namespace UASU_VoucherApprovals.Pages.Officials;

[Authorize(Policy = "TreasuryAdmin")]
public class EditModel : PageModel
{
    private readonly IReferenceDataService _referenceDataService;

    public EditModel(IReferenceDataService referenceDataService)
    {
        _referenceDataService = referenceDataService;
    }

    [BindProperty(SupportsGet = true)]
    public string OfficialId { get; set; } = string.Empty;

    [BindProperty]
    public OfficialInputModel Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var existing = await _referenceDataService.GetOfficialForEditAsync(OfficialId);
        if (existing is null)
            return NotFound();

        Input = existing;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        await _referenceDataService.UpdateOfficialAsync(OfficialId, Input);

        // Redirect (PRG) back to the list so a refresh doesn't resubmit.
        // The key here matches IndexModel's [TempData] StatusMessage
        // property by name, so it shows up there after the redirect.
        TempData["StatusMessage"] = $"Updated {Input.FullName} ({OfficialId}).";
        return RedirectToPage("/Officials/Index");
    }
}
