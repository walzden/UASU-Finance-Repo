using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UASU_VoucherApprovals.Models;
using UASU_VoucherApprovals.Services;

namespace UASU_VoucherApprovals.Pages.MemberRegister;

// Contributor roster management - same gating as Officials/Suppliers/
// Budget Codes, reference data Approvers never need to touch.
[Authorize(Policy = "TreasuryAdmin")]
public class IndexModel : PageModel
{
    private readonly IMemberRegisterService _memberRegisterService;

    public IndexModel(IMemberRegisterService memberRegisterService)
    {
        _memberRegisterService = memberRegisterService;
    }

    [BindProperty]
    public ContributorInputModel Input { get; set; } = new();

    public IEnumerable<ContributorRow> Contributors { get; set; } = Enumerable.Empty<ContributorRow>();

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task OnGetAsync()
    {
        Contributors = await _memberRegisterService.GetContributorsAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            Contributors = await _memberRegisterService.GetContributorsAsync();
            return Page();
        }

        try
        {
            var contributorId = await _memberRegisterService.CreateContributorAsync(Input);
            StatusMessage = $"Added {Input.Full_Name} ({contributorId}).";
        }
        catch (Microsoft.Data.SqlClient.SqlException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            Contributors = await _memberRegisterService.GetContributorsAsync();
            return Page();
        }

        // Redirect (PRG) so a refresh doesn't try to create a duplicate.
        return RedirectToPage();
    }
}
