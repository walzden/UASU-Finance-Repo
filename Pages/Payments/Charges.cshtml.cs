using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using UASU_VoucherApprovals.Models;
using UASU_VoucherApprovals.Services;

namespace UASU_VoucherApprovals.Pages.Payments;

// Log M-Pesa (or other) charges against payments: pick the withdrawal
// that funded them, then tick the payments from a list showing the payee,
// each with its own fee. One charge row per ticked payment, as before -
// trg_PaymentCharges_ValidateWithdrawal still checks each one.
[Authorize(Policy = "TreasuryAdmin")]
public class ChargesModel : PageModel
{
    private readonly IBankingService _bankingService;

    public ChargesModel(IBankingService bankingService)
    {
        _bankingService = bankingService;
    }

    // Which withdrawal's payments are listed. Blank = payments not funded
    // from any withdrawal.
    [BindProperty(SupportsGet = true, Name = "withdrawal")]
    public string? WithdrawalId { get; set; }

    [BindProperty]
    public string Charge_Type { get; set; } = "M-PESA Fee"; // Bank Fee | M-PESA Fee | Other (CK_PaymentCharges_ChargeType)

    [BindProperty]
    public string? Notes { get; set; }

    [BindProperty]
    public List<ChargeLineInput> Lines { get; set; } = new();

    public IEnumerable<SimpleOption> Withdrawals { get; set; } = Enumerable.Empty<SimpleOption>();
    public List<ChargeablePaymentRow> Payments { get; set; } = new();

    public string? StatusMessage { get; set; }
    public bool StatusIsError { get; set; }

    // Set just before the post-save redirect and shown once by OnGetAsync;
    // the fresh GET then lists only payments still without a charge.
    private const string SavedMessageKey = "ChargesSaved";

    public async Task OnGetAsync()
    {
        StatusMessage = TempData[SavedMessageKey] as string;
        await LoadAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await LoadAsync();

        // Only payments still listed for this withdrawal can be charged -
        // a stale or altered form can't add anything else.
        var listed = Payments.Select(p => p.Payment_ID).ToHashSet();
        var ticked = Lines.Where(l => l.Selected && listed.Contains(l.Payment_ID)).ToList();

        string? error = null;
        if (ticked.Count == 0)
            error = "Tick at least one payment.";
        else if (ticked.Any(l => l.Amount is null or <= 0))
            error = "Enter a fee greater than zero for every ticked payment: "
                    + string.Join(", ", ticked.Where(l => l.Amount is null or <= 0).Select(l => l.Payment_ID)) + ".";
        else if (Charge_Type is not ("Bank Fee" or "M-PESA Fee" or "Other"))
            error = "Choose a charge type.";

        if (error is not null)
        {
            StatusIsError = true;
            StatusMessage = error;
            KeepPostedLines();
            return Page();
        }

        try
        {
            var ids = await _bankingService.CreateChargesAsync(
                string.IsNullOrWhiteSpace(WithdrawalId) ? null : WithdrawalId,
                Charge_Type, string.IsNullOrWhiteSpace(Notes) ? null : Notes.Trim(),
                ticked.Select(l => (l.Payment_ID, l.Amount!.Value)).ToList());

            TempData[SavedMessageKey] = ids.Count == 1
                ? $"Charge {ids[0]} logged (Sh. {ticked[0].Amount:N2})."
                : $"{ids.Count} charges logged ({ids[0]} to {ids[^1]}), Sh. {ticked.Sum(l => l.Amount!.Value):N2} in total.";
            return RedirectToPage(new { withdrawal = WithdrawalId });
        }
        catch (SqlException ex)
        {
            StatusIsError = true;
            StatusMessage = ex.Message;
            KeepPostedLines();
            return Page();
        }
    }

    private async Task LoadAsync()
    {
        var withdrawalId = string.IsNullOrWhiteSpace(WithdrawalId) ? null : WithdrawalId;
        Withdrawals = await _bankingService.GetWithdrawalsForChargesAsync(withdrawalId);
        Payments = (await _bankingService.GetChargeablePaymentsAsync(withdrawalId)).ToList();
    }

    // After a refused save, show the ticks and fees as they were typed
    // rather than resetting to the suggestions.
    public Dictionary<string, ChargeLineInput> Posted { get; private set; } = new();
    private void KeepPostedLines() =>
        Posted = Lines.Where(l => !string.IsNullOrEmpty(l.Payment_ID))
                      .GroupBy(l => l.Payment_ID).ToDictionary(g => g.Key, g => g.First());
}
