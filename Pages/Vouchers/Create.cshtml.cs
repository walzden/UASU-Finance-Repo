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
    private readonly IVoucherService _voucherService;
    private readonly IDebtService _debtService;

    public CreateModel(IVoucherService voucherService, IDebtService debtService)
    {
        _voucherService = voucherService;
        _debtService = debtService;
    }

    [BindProperty]
    public VoucherInputModel Input { get; set; } = new();

    public IEnumerable<SimpleOption> BudgetCodes { get; set; } = Enumerable.Empty<SimpleOption>();
    public IEnumerable<OfficialOption> Officials { get; set; } = Enumerable.Empty<OfficialOption>();
    public IEnumerable<SimpleOption> Suppliers { get; set; } = Enumerable.Empty<SimpleOption>();
    public IEnumerable<DebtOption> OutstandingDebts { get; set; } = Enumerable.Empty<DebtOption>();
    public IEnumerable<RejectedVoucherRow> RejectedVouchers { get; set; } = Enumerable.Empty<RejectedVoucherRow>();

    public string? StatusMessage { get; set; }
    public bool StatusIsError { get; set; }

    public async Task OnGetAsync()
    {
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
            var voucherId = await _voucherService.CreateVoucherAsync(Input);
            StatusMessage = $"Voucher {voucherId} created.";
            Input = new VoucherInputModel();

            // Resetting Input alone isn't enough - the asp-for tag helpers
            // read from ModelState first (so a validation error can show
            // back what was typed), and model binding already populated
            // it with the just-submitted values. Without this, the form
            // would still display the old voucher's details, inviting an
            // accidental duplicate submission.
            ModelState.Clear();
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
        BudgetCodes = await _voucherService.GetBudgetCodesAsync();
        Officials = await _voucherService.GetOfficialsAsync();
        Suppliers = await _voucherService.GetSuppliersAsync();
        OutstandingDebts = await _debtService.GetOutstandingDebtsAsync();
        RejectedVouchers = await _voucherService.GetRejectedVouchersAwaitingCorrectionAsync();
    }
}
