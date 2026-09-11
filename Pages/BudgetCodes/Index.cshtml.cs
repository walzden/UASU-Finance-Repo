using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UASU_VoucherApprovals.Models;
using UASU_VoucherApprovals.Services;

namespace UASU_VoucherApprovals.Pages.BudgetCodes;

// Same gating as Officials/Suppliers - reference data that feeds the
// Budget Category dropdown on vouchers and the Proposed Budgets/Budget
// Performance pages, not something Approvers need to touch.
[Authorize(Policy = "TreasuryAdmin")]
public class IndexModel : PageModel
{
    private readonly IReferenceDataService _referenceDataService;

    public IndexModel(IReferenceDataService referenceDataService)
    {
        _referenceDataService = referenceDataService;
    }

    [BindProperty]
    public BudgetCodeInputModel Input { get; set; } = new();

    public IEnumerable<BudgetCodeRow> BudgetCodes { get; set; } = Enumerable.Empty<BudgetCodeRow>();

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task OnGetAsync()
    {
        BudgetCodes = await _referenceDataService.GetBudgetCodeRowsAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            BudgetCodes = await _referenceDataService.GetBudgetCodeRowsAsync();
            return Page();
        }

        var budgetId = await _referenceDataService.CreateBudgetCodeAsync(Input);
        StatusMessage = $"Added {Input.Category_Name} ({budgetId}).";

        // Redirect (PRG) so a refresh doesn't try to create a duplicate.
        return RedirectToPage();
    }
}
