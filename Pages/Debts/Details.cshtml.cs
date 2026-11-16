using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UASU_VoucherApprovals.Models;
using UASU_VoucherApprovals.Services;

namespace UASU_VoucherApprovals.Pages.Debts;

// Read-only, so open to any signed-in user - same as Reports/Debts,
// which links here from each debt's ID.
[Authorize]
public class DetailsModel : PageModel
{
    private readonly IDebtService _debtService;

    public DetailsModel(IDebtService debtService)
    {
        _debtService = debtService;
    }

    public DebtDetail Detail { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(string? id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return RedirectToPage("/Reports/Debts");

        var detail = await _debtService.GetDebtDetailAsync(id);
        if (detail is null)
            return NotFound();

        Detail = detail;
        return Page();
    }
}
