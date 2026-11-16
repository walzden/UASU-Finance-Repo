using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using UASU_VoucherApprovals.Models;
using UASU_VoucherApprovals.Services;
using UASU_VoucherApprovals.Utilities;

namespace UASU_VoucherApprovals.Pages.Vouchers;

[Authorize(Policy = "TreasuryAdmin")] // Treasurer/Deputy Treasurer only - approval is the separate gate for Chairman/Chapter Secretary
public class CreateModel : PageModel
{
    private const string CreatedMessageKey = "VoucherCreatedMessage";

    private readonly IVoucherService _voucherService;
    private readonly IDebtService _debtService;

    public CreateModel(IVoucherService voucherService, IDebtService debtService)
    {
        _voucherService = voucherService;
        _debtService = debtService;
    }

    [BindProperty]
    public VoucherInputModel Input { get; set; } = new();

    // Ticked by the Treasurer after reviewing the possible-duplicate
    // warning. Deliberately not part of VoucherInputModel and never
    // pre-ticked when the page re-renders, so each warning needs its own
    // conscious confirmation.
    [BindProperty]
    public bool ConfirmNotDuplicate { get; set; }

    public IEnumerable<SimpleOption> BudgetCodes { get; set; } = Enumerable.Empty<SimpleOption>();
    public IEnumerable<OfficialOption> Officials { get; set; } = Enumerable.Empty<OfficialOption>();
    public IEnumerable<SimpleOption> Suppliers { get; set; } = Enumerable.Empty<SimpleOption>();
    public IEnumerable<DebtOption> OutstandingDebts { get; set; } = Enumerable.Empty<DebtOption>();
    public IEnumerable<RejectedVoucherRow> RejectedVouchers { get; set; } = Enumerable.Empty<RejectedVoucherRow>();
    public IReadOnlyList<PossibleDuplicateRow> PossibleDuplicates { get; set; } = Array.Empty<PossibleDuplicateRow>();

    public string? StatusMessage { get; set; }
    public bool StatusIsError { get; set; }

    private string CurrentOfficialId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    public async Task OnGetAsync()
    {
        // Set by OnPostAsync just before it redirects here after a save.
        StatusMessage = TempData[CreatedMessageKey] as string;
        await LoadDropdownsAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await LoadDropdownsAsync();

        // When a debt is selected, VoucherService.CreateVoucherAsync
        // derives Payee_Category/Official_Link/Supplier_Link from the
        // debt itself and ignores whatever these fields currently hold -
        // so there's nothing to validate here in that case.
        if (string.IsNullOrWhiteSpace(Input.Debt_Link))
        {
            // Mirror CK_Voucher_PayeeCategory here so the user gets a
            // clear message instead of a raw constraint-violation error.
            VoucherFormValidation.NormalizePayeeFields(Input, ModelState);
        }

        // Travel is orthogonal to payee/debt - runs regardless of which
        // branch above fired. Mirrors CK_Vouchers_TravelDetails.
        VoucherFormValidation.NormalizeTravelFields(Input, ModelState);

        // Income-only auto-payment fields - orthogonal to the above too.
        VoucherFormValidation.NormalizeIncomePaymentFields(Input, ModelState);

        if (!ModelState.IsValid)
            return Page();

        try
        {
            // Same payee and amount within 30 days, or the same
            // description - stop and show them unless the Treasurer has
            // already looked and confirmed this is a separate payment.
            if (!ConfirmNotDuplicate)
            {
                PossibleDuplicates = await _voucherService.FindPossibleDuplicatesAsync(Input);
                if (PossibleDuplicates.Count > 0)
                    return Page();
            }

            var voucherId = await _voucherService.CreateVoucherAsync(Input);

            // Post/Redirect/Get: the browser ends up on a plain GET of a
            // blank form, so pressing Refresh (or Back then Forward)
            // can't resubmit this voucher a second time.
            TempData[CreatedMessageKey] = $"Voucher {voucherId} created.";
            return RedirectToPage();
        }
        catch (SqlException ex)
        {
            StatusIsError = true;
            StatusMessage = ex.Message;
        }

        return Page();
    }

    // Void a rejected voucher that should never be resubmitted (most often
    // a duplicate) so it stops showing as "awaiting correction".
    public async Task<IActionResult> OnPostVoidAsync(string voucherId, string? reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            StatusIsError = true;
            StatusMessage = "Give a reason for voiding the voucher.";
        }
        else
        {
            try
            {
                await _voucherService.VoidVoucherAsync(voucherId, CurrentOfficialId, reason);
                TempData[CreatedMessageKey] = $"Voucher {voucherId} voided.";
                return RedirectToPage();
            }
            catch (Exception ex) when (ex is SqlException or InvalidOperationException)
            {
                StatusIsError = true;
                StatusMessage = ex.Message;
            }
        }

        // Nothing typed into the voucher form belongs to this post.
        ModelState.Clear();
        Input = new VoucherInputModel();
        await LoadDropdownsAsync();
        return Page();
    }

    private async Task LoadDropdownsAsync()
    {
        BudgetCodes = await _voucherService.GetBudgetCodesAsync();
        Officials = await _voucherService.GetOfficialsAsync();
        Suppliers = await _voucherService.GetSuppliersAsync();
        OutstandingDebts = await _debtService.GetOutstandingDebtsAsync();
        RejectedVouchers = await _voucherService.GetRejectedVouchersAwaitingCorrectionAsync();
    }
}
