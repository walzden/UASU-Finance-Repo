using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using UASU_VoucherApprovals.Models;
using UASU_VoucherApprovals.Services;
using UASU_VoucherApprovals.Utilities;

namespace UASU_VoucherApprovals.Pages.Floats;

// TreasuryAdmin only, like every other page that creates a record.
// Spending from a float still needs Chairman + Chapter Secretary approval:
// it reaches the books only through retirement vouchers (see Details).
[Authorize(Policy = "TreasuryAdmin")]
public class IndexModel : PageModel
{
    private readonly ICashFloatService _floatService;

    public IndexModel(ICashFloatService floatService)
    {
        _floatService = floatService;
    }

    [BindProperty]
    public CashFloatInputModel Input { get; set; } = new();

    public IEnumerable<CashFloatRow> Floats { get; set; } = Enumerable.Empty<CashFloatRow>();
    public IEnumerable<SimpleOption> Custodians { get; set; } = Enumerable.Empty<SimpleOption>();
    public IEnumerable<SimpleOption> Withdrawals { get; set; } = Enumerable.Empty<SimpleOption>();

    // Success survives the redirect (PRG); an error is shown on the
    // re-rendered page only, so it isn't replayed on the next visit.
    [TempData]
    public string? StatusMessage { get; set; }
    public string? ErrorMessage { get; set; }

    private string CurrentOfficialId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    public async Task OnGetAsync()
    {
        await LoadAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadAsync();
            return Page();
        }

        try
        {
            var floatId = await _floatService.IssueAsync(Input, CurrentOfficialId);
            StatusMessage = $"Float {floatId} issued. Record receipts against it as they come in.";
            return RedirectToPage("Details", new { id = floatId });
        }
        catch (Exception ex) when (ex is SqlException or InvalidOperationException)
        {
            ErrorMessage = ex.Message;
        }

        await LoadAsync();
        return Page();
    }

    public async Task<IActionResult> OnGetExportCsvAsync()
    {
        var rows = await _floatService.GetFloatsAsync();
        var csv = CsvExport.Build(
            new[] { "Float", "Type", "Purpose", "Custodian", "Withdrawal", "Issued On", "Issued", "Spent (receipts)",
                    "Settled", "Returned", "Cash With Custodian", "Closed On" },
            rows.Select(f => new object?[]
            {
                f.Float_ID, f.Float_Type, f.Purpose, f.CustodianName, f.Withdrawal_ID, f.Issue_Date, f.Issued, f.Spent,
                f.Settled, f.Returned, f.CashWithCustodian, f.Closed_Date
            }));

        return File(csv, "text/csv", "CashFloats.csv");
    }

    private async Task LoadAsync()
    {
        Floats = await _floatService.GetFloatsAsync();
        Custodians = await _floatService.GetCustodianOptionsAsync();
        Withdrawals = await _floatService.GetWithdrawalOptionsAsync();
    }
}
