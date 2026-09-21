using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using UASU_VoucherApprovals.Models;
using UASU_VoucherApprovals.Services;

namespace UASU_VoucherApprovals.Pages.Debts;

// TreasuryAdmin only. Raises vouchers from outstanding debts owed to
// officials: one voucher may pay several debts of one official under one
// budget item. The vouchers then go to the Chairman / Chapter Secretary
// like any other; trg_VoucherDebts_Validate (SQL/031) re-checks the rules.
[Authorize(Policy = "TreasuryAdmin")]
public class PayModel : PageModel
{
    private readonly IDebtPaymentService _debtPaymentService;
    private readonly IVoucherService _voucherService;

    public PayModel(IDebtPaymentService debtPaymentService, IVoucherService voucherService)
    {
        _debtPaymentService = debtPaymentService;
        _voucherService = voucherService;
    }

    [BindProperty]
    public List<DebtSelectionInput> Rows { get; set; } = new();

    [BindProperty]
    public DateTime? VoucherDate { get; set; }

    public IReadOnlyList<DebtToPay> Debts { get; set; } = Array.Empty<DebtToPay>();
    public List<SimpleOption> BudgetOptions { get; set; } = new();
    public List<VoucherPlan> Plans { get; set; } = new();
    public List<string> Errors { get; set; } = new();
    public bool ShowPreview { get; set; }

    [TempData]
    public string? StatusMessage { get; set; }

    public string CurrentOfficialId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    public async Task OnGetAsync()
    {
        await LoadAsync();
    }

    public async Task<IActionResult> OnPostPreviewAsync()
    {
        await LoadAsync();

        var labels = BudgetLabels();
        var (selected, errors) = _debtPaymentService.ValidateSelection(Rows, Debts, labels, CurrentOfficialId);
        Errors = errors;
        if (selected.Count == 0 && errors.Count == 0)
            Errors.Add("Tick at least one debt.");

        if (Errors.Count == 0)
        {
            Plans = _debtPaymentService.PlanVouchers(selected, labels);
            ShowPreview = true;
        }

        return Page();
    }

    public async Task<IActionResult> OnPostCreateAsync()
    {
        await LoadAsync();

        try
        {
            var ids = await _debtPaymentService.CreateVouchersAsync(
                Rows, CurrentOfficialId, (VoucherDate ?? DateTime.Today).Date, BudgetLabels());
            StatusMessage = $"Created {ids.Count} voucher(s): {string.Join(", ", ids)}. They now await approval.";
            return RedirectToPage();
        }
        catch (InvalidOperationException ex)
        {
            Errors.Add(ex.Message);
        }
        catch (SqlException ex)
        {
            Errors.Add(ex.Message);
        }

        return Page();
    }

    public DebtSelectionInput PostedFor(string debtId) =>
        Rows.FirstOrDefault(r => r.Debt_ID == debtId) ?? new DebtSelectionInput();

    private IReadOnlyDictionary<string, string> BudgetLabels() =>
        BudgetOptions.ToDictionary(b => b.Id, b => b.Label);

    private async Task LoadAsync()
    {
        Debts = await _debtPaymentService.GetPayableDebtsAsync();
        BudgetOptions = (await _voucherService.GetBudgetCodesAsync()).ToList();
    }
}
