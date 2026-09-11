using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using UASU_VoucherApprovals.Models;
using UASU_VoucherApprovals.Services;

namespace UASU_VoucherApprovals.Pages.Approvals;

// Restricted at the ASP.NET Core level to the two roles that can approve
// at all - the database trigger (trg_Voucher_Approvals_Validate) still
// enforces the same rule independently, so a bug here can't bypass it.
[Authorize(Policy = "Approvers")]
public class IndexModel : PageModel
{
    private readonly IVoucherService _voucherService;

    public IndexModel(IVoucherService voucherService)
    {
        _voucherService = voucherService;
    }

    public record ApprovalGroup(string Budget_ID, string Budget_Category, DateTime WeekStart, List<PendingApproval> Vouchers);

    // Vouchers sharing a Budget Code, with two or more falling in the
    // same Monday-starting week - each gets a card with an "Approve
    // all" button. Everything else (a budget code with only one voucher
    // that week, or no budget code at all) stays in Ungrouped, exactly
    // as the page looked before bulk approval existed.
    public List<ApprovalGroup> Groups { get; set; } = new();
    public List<PendingApproval> Ungrouped { get; set; } = new();

    public string? StatusMessage { get; set; }
    public bool StatusIsError { get; set; }

    private string CurrentOfficialId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;
    private string CurrentRole => User.FindFirstValue(ClaimTypes.Role)!;

    public async Task OnGetAsync()
    {
        await LoadAsync();
    }

    public async Task<IActionResult> OnPostDecideAsync(string voucherId, string status, string? comments)
    {
        try
        {
            await _voucherService.SubmitApprovalAsync(voucherId, CurrentOfficialId, status, comments);
            StatusMessage = $"Voucher {voucherId} marked {status.ToLower()}.";
        }
        catch (SqlException ex)
        {
            // Surfaces the RAISERROR message from trg_Voucher_Approvals_Validate
            // (e.g. a duplicate approval) directly to the user.
            StatusIsError = true;
            StatusMessage = ex.Message;
        }

        await LoadAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostBulkApproveAsync(string[] voucherIds, string? comments)
    {
        var results = (await _voucherService.BulkApproveAsync(voucherIds, CurrentOfficialId, comments)).ToList();

        var succeeded = results.Count(r => r.Success);
        var failed = results.Where(r => !r.Success).ToList();

        if (failed.Count == 0)
        {
            StatusMessage = $"Approved all {succeeded} voucher(s).";
        }
        else
        {
            // Partial success still counts as success, not an error
            // banner - the ones that went through are done; only flag
            // the banner red if NONE of them made it.
            StatusIsError = succeeded == 0;
            StatusMessage = succeeded > 0
                ? $"Approved {succeeded} voucher(s). {failed.Count} could not be approved: " +
                  string.Join("; ", failed.Select(f => $"{f.Voucher_ID} ({f.Message})"))
                : $"Could not approve any of the {failed.Count} voucher(s): " +
                  string.Join("; ", failed.Select(f => $"{f.Voucher_ID} ({f.Message})"));
        }

        await LoadAsync();
        return Page();
    }

    private async Task LoadAsync()
    {
        var approvals = (await _voucherService.GetPendingApprovalsAsync(CurrentRole, CurrentOfficialId)).ToList();

        var byGroup = approvals
            .Where(a => !string.IsNullOrEmpty(a.Budget_ID))
            .GroupBy(a => (a.Budget_ID, a.WeekStart))
            .Where(g => g.Count() > 1)
            .Select(g => new ApprovalGroup(g.Key.Budget_ID!, g.First().Budget_Category ?? g.Key.Budget_ID!, g.Key.WeekStart, g.OrderBy(a => a.VoucherDate).ToList()))
            .OrderBy(g => g.WeekStart).ThenBy(g => g.Budget_Category)
            .ToList();

        var groupedVoucherIds = byGroup.SelectMany(g => g.Vouchers.Select(v => v.Voucher_ID)).ToHashSet();

        Groups = byGroup;
        Ungrouped = approvals.Where(a => !groupedVoucherIds.Contains(a.Voucher_ID)).ToList();
    }
}
