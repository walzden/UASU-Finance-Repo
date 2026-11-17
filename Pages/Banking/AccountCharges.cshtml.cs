using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using UASU_VoucherApprovals.Models;
using UASU_VoucherApprovals.Services;

namespace UASU_VoucherApprovals.Pages.Banking;

[Authorize(Policy = "TreasuryAdmin")]
public class AccountChargesModel : PageModel
{
    private readonly IBankingService _bankingService;

    public AccountChargesModel(IBankingService bankingService)
    {
        _bankingService = bankingService;
    }

    [BindProperty]
    public AccountChargeInputModel Input { get; set; } = new();

    public IEnumerable<AccountChargeRow> Charges { get; set; } = Enumerable.Empty<AccountChargeRow>();

    // One row per distinct Bank_Account, for the summary cards - grouped
    // here rather than with a separate query, since GetAccountChargesAsync
    // already has every row needed to compute it.
    public IEnumerable<(string BankAccount, decimal Total)> TotalsByAccount =>
        Charges.GroupBy(c => c.Bank_Account)
               .Select(g => (BankAccount: g.Key, Total: g.Sum(c => c.Charge_Amount)))
               .OrderByDescending(t => t.Total);

    public decimal GrandTotal => Charges.Sum(c => c.Charge_Amount);

    public string? StatusMessage { get; set; }
    public bool StatusIsError { get; set; }

    public async Task OnGetAsync()
    {
        StatusMessage = TempData[SavedMessageKey] as string;
        Charges = await _bankingService.GetAccountChargesAsync();
    }

    // Set just before the post-save redirect, shown once by OnGetAsync.
    private const string SavedMessageKey = "AccountChargeSaved";

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            Charges = await _bankingService.GetAccountChargesAsync();
            return Page();
        }

        try
        {
            var chargeId = await _bankingService.CreateAccountChargeAsync(Input);

            // Post/Redirect/Get: the browser lands on a plain GET, so a
            // Refresh reloads the page instead of logging the charge again.
            TempData[SavedMessageKey] = $"Charge {chargeId} logged.";
            return RedirectToPage();
        }
        catch (SqlException ex)
        {
            StatusIsError = true;
            StatusMessage = ex.Message;
        }

        Charges = await _bankingService.GetAccountChargesAsync();
        return Page();
    }
}
