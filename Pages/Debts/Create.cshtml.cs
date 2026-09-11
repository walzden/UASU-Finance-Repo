using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using UASU_VoucherApprovals.Models;
using UASU_VoucherApprovals.Services;

namespace UASU_VoucherApprovals.Pages.Debts;

[Authorize(Policy = "TreasuryAdmin")]
public class CreateModel : PageModel
{
    private readonly IDebtService _debtService;
    private readonly IVoucherService _voucherService;

    public CreateModel(IDebtService debtService, IVoucherService voucherService)
    {
        _debtService = debtService;
        _voucherService = voucherService;
    }

    [BindProperty]
    public DebtInputModel Input { get; set; } = new();

    public IEnumerable<OfficialOption> Officials { get; set; } = Enumerable.Empty<OfficialOption>();
    public IEnumerable<SimpleOption> Suppliers { get; set; } = Enumerable.Empty<SimpleOption>();

    public string? StatusMessage { get; set; }
    public bool StatusIsError { get; set; }

    public async Task OnGetAsync()
    {
        await LoadDropdownsAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await LoadDropdownsAsync();

        // Mirror CK_Debt_CreditorType here so the user gets a clear
        // message instead of a raw constraint-violation error.
        if (Input.Creditor_Type == "Official")
        {
            Input.Supplier_link = null;
            if (string.IsNullOrWhiteSpace(Input.Official_link))
                ModelState.AddModelError(nameof(Input.Official_link), "Select an official.");
        }
        else
        {
            Input.Official_link = null;
            if (string.IsNullOrWhiteSpace(Input.Supplier_link))
                ModelState.AddModelError(nameof(Input.Supplier_link), "Select a supplier.");
        }

        if (!ModelState.IsValid)
            return Page();

        try
        {
            var debtId = await _debtService.CreateDebtAsync(Input);
            StatusMessage = $"Debt {debtId} recorded.";
            Input = new DebtInputModel();

            // See Vouchers/Create.cshtml.cs for why this is also needed -
            // the asp-for tag helpers read from ModelState before the
            // model, so resetting Input alone wouldn't clear the form.
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
        Officials = await _voucherService.GetOfficialsAsync();
        Suppliers = await _voucherService.GetSuppliersAsync();
    }
}
