using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using UASU_VoucherApprovals.Models;
using UASU_VoucherApprovals.Services;

namespace UASU_VoucherApprovals.Pages.Payments;

[Authorize(Policy = "TreasuryAdmin")]
public class ChargesModel : PageModel
{
    private readonly IBankingService _bankingService;

    public ChargesModel(IBankingService bankingService)
    {
        _bankingService = bankingService;
    }

    [BindProperty]
    public PaymentChargeInputModel Input { get; set; } = new();

    public IEnumerable<SimpleOption> Payments { get; set; } = Enumerable.Empty<SimpleOption>();
    public IEnumerable<SimpleOption> Withdrawals { get; set; } = Enumerable.Empty<SimpleOption>();

    // TempData, not a plain property - a successful charge redirects
    // back to this same page (so the Payments dropdown reloads without
    // the just-charged payment still listed), and a plain property
    // wouldn't survive that redirect to the fresh GET request.
    [TempData]
    public string? StatusMessage { get; set; }
    [TempData]
    public bool StatusIsError { get; set; }

    public async Task OnGetAsync()
    {
        await LoadDropdownsAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await LoadDropdownsAsync();

        if (string.IsNullOrWhiteSpace(Input.Withdrawal_ID))
            Input.Withdrawal_ID = null;

        if (!ModelState.IsValid)
            return Page();

        try
        {
            var chargeId = await _bankingService.CreateChargeAsync(Input);
            StatusMessage = $"Charge {chargeId} logged.";

            // Redirect (PRG) rather than just re-rendering Page() here -
            // Payments was already loaded at the top of this method,
            // BEFORE the charge above took the just-charged payment off
            // the eligible list, so rendering directly would still show
            // it as chargeable. A fresh GET re-runs LoadDropdownsAsync
            // and gets the real, current list - without this, the same
            // payment could plausibly get charged twice.
            return RedirectToPage();
        }
        catch (SqlException ex)
        {
            // Surfaces trg_PaymentCharges_ValidateWithdrawal's message
            // directly - e.g. picking a withdrawal that never actually
            // funded the selected payment.
            StatusIsError = true;
            StatusMessage = ex.Message;
        }

        return Page();
    }

    private async Task LoadDropdownsAsync()
    {
        Payments = await _bankingService.GetRecentPaymentsAsync();
        Withdrawals = await _bankingService.GetAllWithdrawalsAsync();
    }
}
