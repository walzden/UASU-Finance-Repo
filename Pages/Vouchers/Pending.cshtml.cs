using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using UASU_VoucherApprovals.Models;
using UASU_VoucherApprovals.Services;
using UASU_VoucherApprovals.Utilities;

namespace UASU_VoucherApprovals.Pages.Vouchers;

// TreasuryAdmin only - same gate as New Voucher, since this is really
// just a correction to what they already entered. A voucher only ever
// offers Edit here while it has zero approvals on file (see
// PendingVoucherRow.CanEdit); trg_Vouchers_BlockEditAfterApproval
// (SQL/020) enforces the same rule at the database layer regardless of
// what this page offers, so an approver's decision can never be
// undermined by a field changing out from under it.
[Authorize(Policy = "TreasuryAdmin")]
public class PendingModel : PageModel
{
    private readonly IVoucherService _voucherService;

    public PendingModel(IVoucherService voucherService)
    {
        _voucherService = voucherService;
    }

    [BindProperty]
    public VoucherInputModel Input { get; set; } = new();

    // Which voucher the submitted edit form belongs to - not part of
    // VoucherInputModel itself, since that shape is shared with
    // CreateVoucherAsync (a brand-new voucher has no ID yet).
    [BindProperty]
    public string VoucherId { get; set; } = string.Empty;

    public IEnumerable<PendingVoucherRow> Vouchers { get; set; } = Enumerable.Empty<PendingVoucherRow>();
    public IEnumerable<SimpleOption> BudgetCodes { get; set; } = Enumerable.Empty<SimpleOption>();
    public IEnumerable<OfficialOption> Officials { get; set; } = Enumerable.Empty<OfficialOption>();
    public IEnumerable<SimpleOption> Suppliers { get; set; } = Enumerable.Empty<SimpleOption>();

    public string? StatusMessage { get; set; }
    public bool StatusIsError { get; set; }

    public async Task OnGetAsync()
    {
        await LoadAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await LoadAsync();

        if (string.IsNullOrWhiteSpace(Input.Debt_Link))
            VoucherFormValidation.NormalizePayeeFields(Input, ModelState);

        VoucherFormValidation.NormalizeTravelFields(Input, ModelState);

        if (!ModelState.IsValid)
            return Page();

        try
        {
            await _voucherService.UpdateVoucherAsync(Input, VoucherId);
            StatusMessage = $"Voucher {VoucherId} updated.";
            Input = new VoucherInputModel();
            VoucherId = string.Empty;

            // See Vouchers/Create.cshtml.cs for why this is also needed -
            // the asp-for tag helpers read from ModelState before the
            // model, so resetting Input alone wouldn't clear the form.
            ModelState.Clear();

            // Refresh so the just-corrected voucher's row (and its
            // now-updated values, if its edit panel is reopened) reflect
            // the change rather than the list going stale until the next
            // full page load.
            Vouchers = await _voucherService.GetPendingVouchersAsync();
        }
        catch (SqlException ex)
        {
            // Catches trg_Vouchers_BlockEditAfterApproval firing on the
            // rare race where an approval lands between the page
            // rendering "Edit" and the correction being submitted.
            StatusIsError = true;
            StatusMessage = ex.Message;
        }

        return Page();
    }

    private async Task LoadAsync()
    {
        Vouchers = await _voucherService.GetPendingVouchersAsync();
        BudgetCodes = await _voucherService.GetBudgetCodesAsync();
        Officials = await _voucherService.GetOfficialsAsync();
        Suppliers = await _voucherService.GetSuppliersAsync();
    }
}
