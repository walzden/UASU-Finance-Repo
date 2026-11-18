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

    public record ApprovalGroup(string Budget_ID, string Budget_Category, List<PendingApproval> Vouchers)
    {
        public DateTime From => Vouchers.Min(v => v.VoucherDate);
        public DateTime To => Vouchers.Max(v => v.VoucherDate);
    }

    // One card per budget code, whatever week the vouchers fall in (it
    // used to need two or more in the same week, so vouchers spread over
    // several weeks never formed a card). Every code gets a card, even
    // with one voucher; only vouchers with no budget code are left in
    // Ungrouped.
    public List<ApprovalGroup> Groups { get; set; } = new();
    public List<PendingApproval> Ungrouped { get; set; } = new();

    // Everything waiting on this approver, for the "Review & approve all"
    // summary: one line per budget code (whatever the week), largest total
    // first. Approving from there posts to the same BulkApprove handler
    // as the weekly cards, so the database checks each voucher the same way.
    public List<PendingApproval> AllPending { get; set; } = new();
    public record BudgetSummary(string Key, string Label, List<PendingApproval> Vouchers)
    {
        public decimal Total => Vouchers.Sum(v => v.Amount);
        public int OwnCount => Vouchers.Count(v => v.IsOwnVoucher);
    }
    public List<BudgetSummary> ByBudget { get; set; } = new();

    public string? StatusMessage { get; set; }
    public bool StatusIsError { get; set; }

    private string CurrentOfficialId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;
    private string CurrentRole => User.FindFirstValue(ClaimTypes.Role)!;

    public async Task OnGetAsync()
    {
        // Set by the decide/bulk-approve handlers just before redirecting.
        StatusMessage = TempData[DecidedMessageKey] as string;
        StatusIsError = TempData[DecidedIsErrorKey] as bool? ?? false;
        await LoadAsync();
    }

    private const string DecidedMessageKey = "ApprovalMessage";
    private const string DecidedIsErrorKey = "ApprovalIsError";

    // Post/Redirect/Get for both handlers: the decision is already saved
    // (or refused) by now, so send the browser to a plain GET of the queue.
    // A Refresh then just reloads it instead of re-sending the decision.
    private IActionResult BackToQueue()
    {
        TempData[DecidedMessageKey] = StatusMessage;
        TempData[DecidedIsErrorKey] = StatusIsError;
        return RedirectToPage();
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

        return BackToQueue();
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

        return BackToQueue();
    }

    private async Task LoadAsync()
    {
        var approvals = (await _voucherService.GetPendingApprovalsAsync(CurrentRole, CurrentOfficialId)).ToList();

        var byGroup = approvals
            .Where(a => !string.IsNullOrEmpty(a.Budget_ID))
            .GroupBy(a => a.Budget_ID!)
            .Select(g => new ApprovalGroup(g.Key, g.First().Budget_Category ?? g.Key, g.OrderBy(a => a.VoucherDate).ToList()))
            // Same order as the "Review & approve all" summary: largest total first.
            .OrderByDescending(g => g.Vouchers.Sum(v => v.Amount)).ThenBy(g => g.Budget_Category)
            .ToList();

        var groupedVoucherIds = byGroup.SelectMany(g => g.Vouchers.Select(v => v.Voucher_ID)).ToHashSet();

        Groups = byGroup;
        Ungrouped = approvals.Where(a => !groupedVoucherIds.Contains(a.Voucher_ID)).ToList();

        AllPending = approvals;
        ByBudget = approvals
            .GroupBy(a => a.Budget_ID ?? "")
            .Select(g => new BudgetSummary(
                g.Key == "" ? "none" : g.Key,
                g.Key == "" ? "No budget code" : $"{g.Key} · {g.First().Budget_Category ?? g.Key}",
                g.OrderBy(a => a.VoucherDate).ToList()))
            .OrderByDescending(b => b.Total)
            .ToList();
    }
}
