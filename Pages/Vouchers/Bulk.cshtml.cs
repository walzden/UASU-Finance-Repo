using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using UASU_VoucherApprovals.Models;
using UASU_VoucherApprovals.Services;

namespace UASU_VoucherApprovals.Pages.Vouchers;

[Authorize(Policy = "TreasuryAdmin")]
public class BulkModel : PageModel
{
    private readonly IBulkVoucherService _bulkVoucherService;
    private readonly IVoucherService _voucherService;

    public BulkModel(IBulkVoucherService bulkVoucherService, IVoucherService voucherService)
    {
        _bulkVoucherService = bulkVoucherService;
        _voucherService = voucherService;
    }

    [BindProperty]
    public BulkVoucherInputModel Input { get; set; } = new();

    public IEnumerable<SimpleOption> BudgetCodes { get; set; } = Enumerable.Empty<SimpleOption>();
    public List<BulkVoucherCandidateRow> Candidates { get; set; } = new();

    [TempData]
    public string? StatusMessage { get; set; }

    public bool StatusIsError { get; set; }

    public async Task OnGetAsync()
    {
        await LoadAsync();
        // Fresh page: everyone who can be paid starts ticked, so the
        // Treasurer only has to untick the few who aren't part of it.
        Input.Lines = Candidates
            .Select(c => new BulkVoucherLine { OfficialID = c.OfficialID, Selected = !c.HasVoucher })
            .ToList();
    }

    // "Check description" - re-reads who already has a voucher with the
    // description now typed, keeping the ticks and amounts as entered.
    public async Task<IActionResult> OnPostRefreshAsync()
    {
        ModelState.Clear();
        await LoadAsync();
        MergeLines();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await LoadAsync();
        MergeLines();

        var selected = Input.Lines.Where(l => l.Selected).ToList();
        if (selected.Count == 0)
            ModelState.AddModelError(string.Empty, "Tick at least one official.");

        for (var i = 0; i < Input.Lines.Count; i++)
        {
            var line = Input.Lines[i];
            if (line.Selected && (line.Amount is null || line.Amount <= 0))
                ModelState.AddModelError($"Input.Lines[{i}].Amount", "Enter an amount greater than zero.");
        }

        if (!ModelState.IsValid)
            return Page();

        try
        {
            var ids = await _bulkVoucherService.CreateAsync(
                Input.VoucherDate, Input.Budget_Link, Input.Description,
                selected.ToDictionary(l => l.OfficialID, l => l.Amount!.Value));

            StatusMessage =
                $"Created {ids.Count} voucher(s) for \"{Input.Description.Trim()}\" ({ids.First()} to {ids.Last()}), " +
                $"totalling KES {selected.Sum(l => l.Amount!.Value):N2}. They now await approval under Pending Vouchers.";
            return RedirectToPage();
        }
        catch (Exception ex) when (ex is SqlException or InvalidOperationException)
        {
            StatusIsError = true;
            StatusMessage = ex.Message;
            return Page();
        }
    }

    private async Task LoadAsync()
    {
        BudgetCodes = (await _voucherService.GetBudgetCodesAsync())
            .Where(b => b.Label.EndsWith("(Expense)", StringComparison.OrdinalIgnoreCase))
            .ToList();
        Candidates = (await _bulkVoucherService.GetCandidatesAsync(Input.Description)).ToList();
    }

    // Lines are rebuilt in Candidates order so each row lines up with its
    // official even if the list changed since the page was loaded. An
    // official who now already has a voucher is unticked rather than
    // left for CreateAsync to refuse.
    private void MergeLines()
    {
        var posted = Input.Lines
            .Where(l => !string.IsNullOrEmpty(l.OfficialID))
            .GroupBy(l => l.OfficialID)
            .ToDictionary(g => g.Key, g => g.First());

        Input.Lines = Candidates.Select(c =>
        {
            posted.TryGetValue(c.OfficialID, out var p);
            return new BulkVoucherLine
            {
                OfficialID = c.OfficialID,
                Selected = !c.HasVoucher && (p?.Selected ?? false),
                Amount = p?.Amount
            };
        }).ToList();
    }
}
