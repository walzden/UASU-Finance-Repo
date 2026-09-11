using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UASU_VoucherApprovals.Models;
using UASU_VoucherApprovals.Services;

namespace UASU_VoucherApprovals.Pages.BudgetCodes;

[Authorize(Policy = "TreasuryAdmin")]
public class EditModel : PageModel
{
    private readonly IReferenceDataService _referenceDataService;

    public EditModel(IReferenceDataService referenceDataService)
    {
        _referenceDataService = referenceDataService;
    }

    [BindProperty(SupportsGet = true)]
    public string BudgetId { get; set; } = string.Empty;

    [BindProperty]
    public BudgetCodeInputModel Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var existing = await _referenceDataService.GetBudgetCodeForEditAsync(BudgetId);
        if (existing is null)
            return NotFound();

        Input = existing;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        await _referenceDataService.UpdateBudgetCodeAsync(BudgetId, Input);

        // Redirect (PRG) back to the list so a refresh doesn't resubmit.
        TempData["StatusMessage"] = $"Updated {Input.Category_Name} ({BudgetId}).";
        return RedirectToPage("/BudgetCodes/Index");
    }
}
