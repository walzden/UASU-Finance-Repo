using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using UASU_VoucherApprovals.Models;
using UASU_VoucherApprovals.Services;

namespace UASU_VoucherApprovals.Pages.Debts;

// New Debt: a supplier, or officials ticked from a list - one official
// for a personal debt, several for a common debt (one debt each, for
// their share). A debt can cover a single day or a range of days.
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
    public DebtEntryInputModel Input { get; set; } = new();

    public List<OfficialOption> Officials { get; set; } = new();
    public IEnumerable<SimpleOption> Suppliers { get; set; } = Enumerable.Empty<SimpleOption>();

    public string? StatusMessage { get; set; }
    public bool StatusIsError { get; set; }

    private const string SavedMessageKey = "DebtSaved";

    public async Task OnGetAsync()
    {
        StatusMessage = TempData[SavedMessageKey] as string;
        await LoadDropdownsAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await LoadDropdownsAsync();

        var debts = new List<NewDebt>();

        if (Input.Creditor_Type == "Supplier")
        {
            if (string.IsNullOrWhiteSpace(Input.Supplier_link))
                ModelState.AddModelError("Input.Supplier_link", "Select a supplier.");
            if (Input.Supplier_Amount is null or <= 0)
                ModelState.AddModelError("Input.Supplier_Amount", "Enter the amount owed (more than zero).");
            if (ModelState.IsValid)
                debts.Add(new NewDebt("Supplier", null, Input.Supplier_link, Input.Supplier_Amount!.Value));
        }
        else
        {
            Input.Creditor_Type = "Official";
            // Only officials actually on the list can be ticked.
            var known = Officials.Select(o => o.Id).ToHashSet();
            var ticked = Input.Officials.Where(l => l.Selected && known.Contains(l.OfficialID)).ToList();

            if (ticked.Count == 0)
                ModelState.AddModelError("Input.Officials", "Tick at least one official - one for a personal debt, several for a common debt.");

            for (var i = 0; i < Input.Officials.Count; i++)
            {
                var l = Input.Officials[i];
                if (l.Selected && l.Amount is null or <= 0)
                    ModelState.AddModelError($"Input.Officials[{i}].Amount",
                        $"Enter an amount for {Officials.FirstOrDefault(o => o.Id == l.OfficialID)?.FullName ?? l.OfficialID}.");
            }

            if (ModelState.IsValid)
                debts.AddRange(ticked.Select(l => new NewDebt("Official", l.OfficialID, null, l.Amount!.Value)));
        }

        DateTime? dateTo = null;
        if (Input.Spans_Days)
        {
            if (Input.Date_Incurred_To is null)
                ModelState.AddModelError("Input.Date_Incurred_To", "Enter the last day, or choose a single day.");
            else if (Input.Date_Incurred_To.Value.Date < Input.Date_Incurred.Date)
                ModelState.AddModelError("Input.Date_Incurred_To", "The last day can't be before the first day.");
            else if (Input.Date_Incurred_To.Value.Date > Input.Date_Incurred.Date)
                dateTo = Input.Date_Incurred_To.Value.Date;
        }
        if (Input.Date_Incurred.Date > DateTime.Today || dateTo > DateTime.Today)
            ModelState.AddModelError("Input.Date_Incurred", "A debt can't be incurred in the future.");

        if (!ModelState.IsValid)
            return Page();

        try
        {
            var ids = await _debtService.CreateDebtsAsync(debts, Input.Date_Incurred, dateTo, Input.Invoice_No, Input.Description!);

            TempData[SavedMessageKey] = ids.Count == 1
                ? $"Debt {ids[0]} recorded (Sh. {debts[0].Total_Owed:N2})."
                : $"Common debt recorded for {ids.Count} officials ({ids[0]} to {ids[^1]}), Sh. {debts.Sum(d => d.Total_Owed):N2} in total.";
            return RedirectToPage();
        }
        catch (SqlException ex)
        {
            StatusIsError = true;
            StatusMessage = ex.Message;
            return Page();
        }
    }

    private async Task LoadDropdownsAsync()
    {
        // Current officials first, then former (still valid creditors,
        // e.g. arrears owed after leaving office).
        Officials = (await _voucherService.GetOfficialsAsync()).ToList();
        Suppliers = await _voucherService.GetSuppliersAsync();
    }
}
