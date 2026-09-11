using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using UASU_VoucherApprovals.Models;
using UASU_VoucherApprovals.Services;

namespace UASU_VoucherApprovals.Pages.Payments;

[Authorize(Policy = "TreasuryAdmin")]
public class AddAllocationModel : PageModel
{
    private readonly IBankingService _bankingService;
    private readonly IVoucherService _voucherService;

    public AddAllocationModel(IBankingService bankingService, IVoucherService voucherService)
    {
        _bankingService = bankingService;
        _voucherService = voucherService;
    }

    [BindProperty]
    public AddAllocationInputModel Input { get; set; } = new();

    public IEnumerable<SimpleOption> Payments { get; set; } = Enumerable.Empty<SimpleOption>();
    public IEnumerable<SimpleOption> EligibleVouchers { get; set; } = Enumerable.Empty<SimpleOption>();

    // TempData, not a plain property - a successful allocation redirects
    // back to this same page (so Payments/EligibleVouchers reload without
    // whichever just became fully-allocated/fully-paid still listed),
    // and a plain property wouldn't survive that redirect to the fresh
    // GET request.
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

        if (!ModelState.IsValid)
            return Page();

        try
        {
            // trg_PaymentAllocations_RequireApproval and
            // trg_PaymentStatusUpdate both fire here exactly as they
            // do for a brand-new payment.
            await _bankingService.AddAllocationAsync(Input);
            StatusMessage = $"Voucher {Input.Voucher_ID} added to payment {Input.Payment_ID}.";

            // Redirect (PRG) rather than just re-rendering Page() here -
            // Payments/EligibleVouchers were already loaded at the top of
            // this method, BEFORE the allocation above could have fully
            // consumed either one's remaining balance, so rendering
            // directly could still offer an already-settled payment or
            // voucher. A fresh GET re-runs LoadDropdownsAsync and gets
            // the real, current lists.
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
        Payments = await _bankingService.GetPaymentsWithUnallocatedBalanceAsync();
        EligibleVouchers = await _voucherService.GetFullyApprovedUnpaidVouchersAsync();
    }
}
