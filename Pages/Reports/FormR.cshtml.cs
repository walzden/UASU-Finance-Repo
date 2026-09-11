using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UASU_VoucherApprovals.Models;
using UASU_VoucherApprovals.Services;
using UASU_VoucherApprovals.Utilities;

namespace UASU_VoucherApprovals.Pages.Reports;

// [Authorize] only, same as Withdrawal Breakdown - read-only, and
// Approvers have no reason to be blocked from seeing it. One Form R
// per withdrawal, since that's what the physical paper form authorizes -
// BuildFormRDataAsync is the per-withdrawal logic shared by both the
// single-withdrawal view and the "every withdrawal this year" view, so
// the two can never drift into showing different numbers for the same
// withdrawal.
[Authorize]
public class FormRModel : PageModel
{
    private readonly IBankingService _bankingService;
    private readonly IVoucherService _voucherService;

    public FormRModel(IBankingService bankingService, IVoucherService voucherService)
    {
        _bankingService = bankingService;
        _voucherService = voucherService;
    }

    [BindProperty(SupportsGet = true, Name = "mode")]
    public string Mode { get; set; } = "single";

    [BindProperty(SupportsGet = true, Name = "year")]
    public int SelectedYear { get; set; }

    [BindProperty(SupportsGet = true, Name = "withdrawal")]
    public string? SelectedWithdrawalId { get; set; }

    public List<WithdrawalListRow> Withdrawals { get; set; } = new();
    public List<int> AvailableYears { get; set; } = new();
    public List<FormRData> FormRs { get; set; } = new();

    // Aggregated by budget category, same reasoning as the Cash Book
    // report - several vouchers under one category collapse into one
    // PARTICULARS line instead of one per voucher.
    public record ParticularLine(string Category, decimal Amount);

    // Represents "every voucher behind this withdrawal that has a
    // Chairman/Chapter Secretary approval agrees" - Name/Date come from
    // whichever of those approvals is most recent; Approved is only
    // true when there's at least one such approval and none of them are
    // anything other than 'Approved' (a Rejected/Pending record for the
    // same role means this can't honestly be shown as signed off).
    public record ApprovalSummary(string? Name, DateTime? Date, bool Approved);

    // Everything one Form R needs to render, for one withdrawal.
    public record FormRData(
        WithdrawalListRow Withdrawal,
        List<ParticularLine> Particulars,
        decimal Total,
        string AmountInWords,
        ApprovalSummary? ChairmanApproval,
        ApprovalSummary? ChapterSecretaryApproval,
        bool AllVouchersLegacy,
        int LegacyVoucherCount,
        DateTime? TreasurerActionDate,
        List<AcknowledgementRow> Acknowledgements,
        int UnacknowledgedPaymentCount,
        List<string> UnconfirmedPayeeNames);

    public async Task OnGetAsync()
    {
        Withdrawals = (await _bankingService.GetWithdrawalsAsync())
            .OrderByDescending(w => w.Withdrawal_Date)
            .ToList();

        AvailableYears = Withdrawals.Select(w => w.Withdrawal_Date.Year).Distinct().OrderByDescending(y => y).ToList();
        if (SelectedYear == 0)
            SelectedYear = AvailableYears.FirstOrDefault(y => y == DateTime.Today.Year, AvailableYears.FirstOrDefault());

        List<WithdrawalListRow> targets;
        if (Mode == "all")
        {
            targets = Withdrawals.Where(w => w.Withdrawal_Date.Year == SelectedYear).OrderBy(w => w.Withdrawal_Date).ToList();
        }
        else
        {
            if (string.IsNullOrEmpty(SelectedWithdrawalId))
            {
                SelectedWithdrawalId = Withdrawals.FirstOrDefault(w => w.Withdrawal_Date.Year == SelectedYear)?.Withdrawal_ID
                    ?? Withdrawals.FirstOrDefault()?.Withdrawal_ID;
            }

            var single = Withdrawals.FirstOrDefault(w => w.Withdrawal_ID == SelectedWithdrawalId);
            targets = single is not null ? new List<WithdrawalListRow> { single } : new List<WithdrawalListRow>();
        }

        if (targets.Count == 0)
            return;

        // Fetched once regardless of how many withdrawals are being
        // rendered - these don't vary per withdrawal, only the
        // attribution/filtering below does.
        var payments = (await _bankingService.GetWithdrawalPaymentsAsync()).ToList();
        var links = await _bankingService.GetWithdrawalPaymentLinksAsync();
        var vouchers = await _bankingService.GetWithdrawalPaymentVouchersAsync();
        var (vouchersForWithdrawalPayment, _) = WithdrawalBreakdownModel.AttributeVouchers(payments, links, vouchers);

        foreach (var withdrawal in targets)
            FormRs.Add(await BuildFormRDataAsync(withdrawal, payments, vouchersForWithdrawalPayment));
    }

    private async Task<FormRData> BuildFormRDataAsync(
        WithdrawalListRow withdrawal,
        List<WithdrawalPaymentRow> payments,
        Dictionary<(string, string), List<WithdrawalPaymentVoucherRow>> vouchersForWithdrawalPayment)
    {
        var attributedVouchers = payments
            .Where(p => p.Withdrawal_ID == withdrawal.Withdrawal_ID)
            .SelectMany(p => vouchersForWithdrawalPayment.TryGetValue((withdrawal.Withdrawal_ID, p.Payment_ID), out var list)
                ? list
                : new List<WithdrawalPaymentVoucherRow>())
            .ToList();

        var particulars = attributedVouchers
            .GroupBy(v => string.IsNullOrWhiteSpace(v.Category_Name) ? "Uncategorized" : v.Category_Name!)
            .Select(g => new ParticularLine(g.Key, g.Sum(v => v.AllocatedFromThisPayment)))
            .OrderByDescending(p => p.Amount)
            .ToList();

        // Bank/M-PESA charges deducted as part of disbursing this
        // withdrawal - real money that left the bank for this
        // transaction, but never allocated to any voucher, so it has to
        // show as its own line or TOTAL would silently understate what
        // was actually withdrawn. withdrawal.Charges already nets this
        // out correctly (including charges pro-rated across withdrawals
        // for a split payment) via the same BankReconciliation view
        // Withdrawal Breakdown itself reads from.
        if (withdrawal.Charges > 0)
            particulars.Add(new ParticularLine("Bank/M-PESA Transaction Charges", withdrawal.Charges));

        var total = particulars.Sum(p => p.Amount);
        var amountInWords = NumberToWords.ConvertShillings(total);

        var distinctVouchers = attributedVouchers
            .GroupBy(v => v.Voucher_ID)
            .Select(g => g.First())
            .ToList();
        var legacyVoucherCount = distinctVouchers.Count(v => v.Is_Legacy);
        var allVouchersLegacy = distinctVouchers.Count > 0 && legacyVoucherCount == distinctVouchers.Count;

        // Legacy vouchers never had a Voucher_Approvals row to begin
        // with (they predate that workflow entirely) - looking them up
        // would just come back empty and read as "still pending", which
        // is wrong. Only non-legacy vouchers are checked; the view shows
        // AllVouchersLegacy/LegacyVoucherCount to explain the rest.
        var voucherIdsToCheck = distinctVouchers.Where(v => !v.Is_Legacy).Select(v => v.Voucher_ID).ToList();
        var approvals = voucherIdsToCheck.Count > 0
            ? (await _voucherService.GetVoucherApprovalActionsByIdsAsync(voucherIdsToCheck)).ToList()
            : new List<VoucherApprovalActionRow>();

        var chairmanApproval = SummarizeApprovals(approvals, "Chairman");
        var chapterSecretaryApproval = SummarizeApprovals(approvals, "Chapter Secretary");

        var paymentsForWithdrawal = payments.Where(p => p.Withdrawal_ID == withdrawal.Withdrawal_ID).ToList();
        var treasurerActionDate = paymentsForWithdrawal.Count > 0
            ? paymentsForWithdrawal.Min(p => p.Payment_Date)
            : (DateTime?)null;

        var paymentIds = paymentsForWithdrawal.Select(p => p.Payment_ID).Distinct().ToList();
        var acknowledgements = paymentIds.Count > 0
            ? (await _voucherService.GetAcknowledgementsByPaymentIdsAsync(paymentIds)).ToList()
            : new List<AcknowledgementRow>();

        var unacknowledgedPaymentIds = paymentIds.Where(id => acknowledgements.All(a => a.Payment_ID != id)).ToHashSet();

        // Named by payee rather than by Payment_ID, since "who hasn't
        // confirmed yet" is the useful question here.
        var unconfirmedPayeeNames = attributedVouchers
            .Where(v => unacknowledgedPaymentIds.Contains(v.Payment_ID))
            .Select(v => v.PayeeDisplay)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Select(name => name!)
            .Distinct()
            .OrderBy(name => name)
            .ToList();

        return new FormRData(
            withdrawal, particulars, total, amountInWords,
            chairmanApproval, chapterSecretaryApproval,
            allVouchersLegacy, legacyVoucherCount,
            treasurerActionDate,
            acknowledgements, unacknowledgedPaymentIds.Count, unconfirmedPayeeNames);
    }

    private static ApprovalSummary? SummarizeApprovals(List<VoucherApprovalActionRow> approvals, string role)
    {
        var forRole = approvals.Where(a => a.ApproverRole == role).ToList();
        if (forRole.Count == 0)
            return null;

        var latest = forRole.OrderByDescending(a => a.Approval_Date).First();
        var allApproved = forRole.All(a => a.Approval_Status == "Approved");

        return new ApprovalSummary(latest.ApproverName, latest.Approval_Date, allApproved);
    }
}
