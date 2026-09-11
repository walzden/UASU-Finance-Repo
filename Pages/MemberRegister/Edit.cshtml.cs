using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UASU_VoucherApprovals.Models;
using UASU_VoucherApprovals.Services;

namespace UASU_VoucherApprovals.Pages.MemberRegister;

// Corrects a typo in a contributor's PF No./Name after the fact -
// Contributor_ID itself is never editable here (it's the permanent
// generated membership number every year's contribution history is
// keyed against).
[Authorize(Policy = "TreasuryAdmin")]
public class EditModel : PageModel
{
    private readonly IMemberRegisterService _memberRegisterService;

    public EditModel(IMemberRegisterService memberRegisterService)
    {
        _memberRegisterService = memberRegisterService;
    }

    [BindProperty(SupportsGet = true)]
    public string ContributorId { get; set; } = string.Empty;

    [BindProperty]
    public ContributorInputModel Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var existing = await _memberRegisterService.GetContributorForEditAsync(ContributorId);
        if (existing is null)
            return NotFound();

        Input = existing;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        await _memberRegisterService.UpdateContributorAsync(ContributorId, Input);

        // Redirect (PRG) back to the list so a refresh doesn't resubmit.
        TempData["StatusMessage"] = $"Updated {Input.Full_Name} ({ContributorId}).";
        return RedirectToPage("/MemberRegister/Index");
    }
}
