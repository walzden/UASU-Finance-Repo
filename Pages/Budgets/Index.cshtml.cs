using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UASU_VoucherApprovals.Models;
using UASU_VoucherApprovals.Services;

namespace UASU_VoucherApprovals.Pages.Budgets;

// Proposing/adjusting budgets is planning work, same bucket as New
// Voucher/New Debt - Treasurer/Deputy Treasurer prepare it, Chairman/
// Chapter Secretary see the result on Budget Performance under Reports.
[Authorize(Policy = "TreasuryAdmin")]
public class IndexModel : PageModel
{
    private readonly IBudgetPlanningService _budgetPlanningService;

    public IndexModel(IBudgetPlanningService budgetPlanningService)
    {
        _budgetPlanningService = budgetPlanningService;
    }

    [BindProperty(SupportsGet = true, Name = "year")]
    public int FiscalYear { get; set; } = DateTime.Today.Year;

    [BindProperty]
    public ProposedBudgetInputModel Input { get; set; } = new();

    public IEnumerable<ProposedBudgetRow> ProposedBudgets { get; set; } = Enumerable.Empty<ProposedBudgetRow>();
    public IEnumerable<int> AvailableYears { get; set; } = Enumerable.Empty<int>();
    public IEnumerable<SimpleOption> BudgetCodes { get; set; } = Enumerable.Empty<SimpleOption>();

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task OnGetAsync()
    {
        Input.Fiscal_Year = FiscalYear;
        await LoadAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadAsync();
            return Page();
        }

        var proposalId = await _budgetPlanningService.CreateProposedBudgetAsync(Input);
        if (proposalId is null)
        {
            ModelState.AddModelError(string.Empty,
                "A proposed budget already exists for this budget code and fiscal year - edit it instead of adding another.");
            await LoadAsync();
            return Page();
        }

        StatusMessage = $"Added proposed budget for {Input.Fiscal_Year} (Proposal #{proposalId}).";

        // Redirect (PRG), keeping the same year filter, so a refresh
        // doesn't try to create a duplicate.
        return RedirectToPage(new { year = Input.Fiscal_Year });
    }

    private async Task LoadAsync()
    {
        AvailableYears = await _budgetPlanningService.GetAvailableFiscalYearsAsync();
        BudgetCodes = await _budgetPlanningService.GetBudgetCodeOptionsAsync();
        ProposedBudgets = await _budgetPlanningService.GetProposedBudgetsAsync(FiscalYear);
    }
}
