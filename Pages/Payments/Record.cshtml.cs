using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using UASU_VoucherApprovals.Models;
using UASU_VoucherApprovals.Services;

namespace UASU_VoucherApprovals.Pages.Payments;

[Authorize(Policy = "TreasuryAdmin")]
public class RecordModel : PageModel
{
    private readonly IVoucherService _voucherService;
    private readonly IBankingService _bankingService;

    public RecordModel(IVoucherService voucherService, IBankingService bankingService)
    {
        _voucherService = voucherService;
        _bankingService = bankingService;
    }

    [BindProperty]
    public PaymentInputModel Input { get; set; } = new();

    public IEnumerable<ApprovedUnpaidVoucherRow> ApprovedVouchers { get; set; } = Enumerable.Empty<ApprovedUnpaidVoucherRow>();
    public IEnumerable<SimpleOption> AvailableWithdrawals { get; set; } = Enumerable.Empty<SimpleOption>();

    // TempData, not a plain property - a successful payment redirects
    // back to this same page (so ApprovedVouchers reloads without the
    // just-paid vouchers still showing as payable), and a plain property
    // wouldn't survive that redirect to the fresh GET request.
    [TempData]
    public string? StatusMessage { get; set; }
    [TempData]
    public bool StatusIsError { get; set; }

    public async Task OnGetAsync(string? voucherId)
    {
        await LoadDropdownsAsync();

        // A single-voucher deep link (kept for compatibility with any
        // existing link/bookmark) pre-checks that one voucher rather
        // than making the user find it again in the list.
        if (!string.IsNullOrWhiteSpace(voucherId))
            Input.Voucher_IDs = new List<string> { voucherId };
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await LoadDropdownsAsync();

        if (string.IsNullOrWhiteSpace(Input.Withdrawal_Link))
            Input.Withdrawal_Link = null;

        if (!ModelState.IsValid)
            return Page();

        try
        {
            // trg_PaymentAllocations_RequireApproval and trg_PaymentStatusUpdate
            // fire as a final server-side check even though the list was
            // already filtered to eligible vouchers.
            var voucherCount = Input.Voucher_IDs.Count;
            var paymentId = await _voucherService.RecordPaymentAsync(Input);
            StatusMessage = voucherCount == 1
                ? $"Payment {paymentId} recorded against voucher {Input.Voucher_IDs[0]}."
                : $"Payment {paymentId} recorded against {voucherCount} vouchers.";

            // Redirect (PRG) rather than just re-rendering Page() here -
            // ApprovedVouchers was already loaded at the top of this
            // method, BEFORE the payment above took the just-paid
            // vouchers off the eligible list, so rendering directly would
            // show them as if they were still payable. A fresh GET
            // re-runs LoadDropdownsAsync and gets the real, current list -
            // without this, someone could plausibly re-select an
            // already-paid voucher into a second payment.
            return RedirectToPage();
        }
        catch (SqlException ex)
        {
            StatusIsError = true;
            StatusMessage = ex.Message;
        }

        return Page();
    }

    private async Task LoadDropdownsAsync()
    {
        ApprovedVouchers = await _voucherService.GetApprovedUnpaidVouchersDetailedAsync();
        AvailableWithdrawals = await _bankingService.GetWithdrawalsWithBalanceAsync();
    }
}
