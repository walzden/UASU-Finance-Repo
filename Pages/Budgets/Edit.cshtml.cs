using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UASU_VoucherApprovals.Models;
using UASU_VoucherApprovals.Services;

namespace UASU_VoucherApprovals.Pages.Budgets;

[Authorize(Policy = "TreasuryAdmin")]
public class EditModel : PageModel
{
    private readonly IBudgetPlanningService _budgetPlanningService;

    public EditModel(IBudgetPlanningService budgetPlanningService)
    {
        _budgetPlanningService = budgetPlanningService;
    }

    [BindProperty(SupportsGet = true)]
    public int ProposalId { get; set; }

    [BindProperty]
    public ProposedBudgetInputModel Input { get; set; } = new();

    public IEnumerable<SimpleOption> BudgetCodes { get; set; } = Enumerable.Empty<SimpleOption>();

    public async Task<IActionResult> OnGetAsync()
    {
        var existing = await _budgetPlanningService.GetProposedBudgetForEditAsync(ProposalId);
        if (existing is null)
            return NotFound();

        Input = existing;
        BudgetCodes = await _budgetPlanningService.GetBudgetCodeOptionsAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            BudgetCodes = await _budgetPlanningService.GetBudgetCodeOptionsAsync();
            return Page();
        }

        await _budgetPlanningService.UpdateProposedBudgetAsync(ProposalId, Input);

        // Redirect (PRG) back to the list, filtered to this proposal's
        // fiscal year, so a refresh doesn't resubmit.
        TempData["StatusMessage"] = $"Updated proposed budget for {Input.Fiscal_Year}.";
        return RedirectToPage("/Budgets/Index", new { year = Input.Fiscal_Year });
    }
}
