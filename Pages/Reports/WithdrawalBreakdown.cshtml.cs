using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UASU_VoucherApprovals.Models;
using UASU_VoucherApprovals.Services;

namespace UASU_VoucherApprovals.Pages.Reports;

// Open to any signed-in user, same as the other reports - it's read-only
// and Approvers have no reason to be blocked from seeing where the
// union's cash actually went.
[Authorize]
public class WithdrawalBreakdownModel : PageModel
{
    private readonly IBankingService _bankingService;

    public WithdrawalBreakdownModel(IBankingService bankingService)
    {
        _bankingService = bankingService;
    }

    public IEnumerable<WithdrawalListRow> Withdrawals { get; set; } = Enumerable.Empty<WithdrawalListRow>();
    public ILookup<string, WithdrawalPaymentRow> PaymentsByWithdrawal { get; set; } = Enumerable.Empty<WithdrawalPaymentRow>().ToLookup(p => p.Withdrawal_ID);

    // Keyed by (Withdrawal_ID, Payment_ID) - the vouchers this specific
    // withdrawal is attributed as having funded within this payment, not
    // the payment's full voucher list (see AttributeVouchers below).
    public Dictionary<(string WithdrawalId, string PaymentId), List<WithdrawalPaymentVoucherRow>> VouchersForWithdrawalPayment { get; set; } = new();

    // Payment_IDs where amount-matching couldn't confidently attribute
    // vouchers to a specific withdrawal - the view falls back to showing
    // the payment's full voucher list under every withdrawal that funded
    // it, and flags it rather than silently guessing.
    public HashSet<string> UnattributedPayments { get; set; } = new();

    // The withdrawal-level "Description" column - distinct budget
    // category names across every voucher attributed to that withdrawal
    // (via VouchersForWithdrawalPayment), sorted, not one entry per
    // voucher - a withdrawal that funded seven Meeting Allowance
    // vouchers still shows that category once.
    public Dictionary<string, List<string>> BudgetCategoriesByWithdrawal { get; set; } = new();

    public async Task OnGetAsync()
    {
        Withdrawals = await _bankingService.GetWithdrawalsAsync();
        var payments = await _bankingService.GetWithdrawalPaymentsAsync();
        var links = await _bankingService.GetWithdrawalPaymentLinksAsync();
        var vouchers = await _bankingService.GetWithdrawalPaymentVouchersAsync();

        PaymentsByWithdrawal = payments.ToLookup(p => p.Withdrawal_ID);
        (VouchersForWithdrawalPayment, UnattributedPayments) = AttributeVouchers(payments, links, vouchers);
        BudgetCategoriesByWithdrawal = BuildBudgetCategoriesByWithdrawal(Withdrawals, PaymentsByWithdrawal, VouchersForWithdrawalPayment);
    }

    private static Dictionary<string, List<string>> BuildBudgetCategoriesByWithdrawal(
        IEnumerable<WithdrawalListRow> withdrawals,
        ILookup<string, WithdrawalPaymentRow> paymentsByWithdrawal,
        Dictionary<(string, string), List<WithdrawalPaymentVoucherRow>> vouchersForWithdrawalPayment)
    {
        var result = new Dictionary<string, List<string>>();

        foreach (var w in withdrawals)
        {
            var categories = paymentsByWithdrawal[w.Withdrawal_ID]
                .SelectMany(p => vouchersForWithdrawalPayment.TryGetValue((w.Withdrawal_ID, p.Payment_ID), out var list) ? list : new List<WithdrawalPaymentVoucherRow>())
                .Select(v => v.Category_Name)
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Distinct()
                .OrderBy(c => c)
                .Select(c => c!)
                .ToList();

            result[w.Withdrawal_ID] = categories;
        }

        return result;
    }

    // A payment can be funded from more than one withdrawal, but there's
    // no column that says which voucher a given withdrawal's cash
    // actually paid for - WithdrawalPayments and PaymentAllocations are
    // both just links to Payments, not to each other. In practice, every
    // withdrawal-funding link recorded so far carries the exact same
    // amount as exactly one voucher allocation on that payment (verified
    // against the live data before building this), so matching by
    // amount recovers the attribution reliably. Where a payment's link
    // amounts and voucher amounts don't line up as the same multiset -
    // which would mean this assumption doesn't hold for that payment -
    // it's marked unattributed rather than guessing.
    // internal (not private) so FormRModel can reuse the exact same
    // attribution rule - a withdrawal's Form R line items should never
    // drift out of sync with what Withdrawal Breakdown shows for it.
    internal static (Dictionary<(string, string), List<WithdrawalPaymentVoucherRow>>, HashSet<string>) AttributeVouchers(
        IEnumerable<WithdrawalPaymentRow> payments,
        IEnumerable<WithdrawalPaymentLinkRow> links,
        IEnumerable<WithdrawalPaymentVoucherRow> vouchers)
    {
        var result = new Dictionary<(string, string), List<WithdrawalPaymentVoucherRow>>();
        var unattributed = new HashSet<string>();

        var linksByPayment = links.ToLookup(l => l.Payment_ID);
        var vouchersByPayment = vouchers.ToLookup(v => v.Payment_ID);

        foreach (var paymentId in payments.Select(p => p.Payment_ID).Distinct())
        {
            var paymentLinks = linksByPayment[paymentId].ToList();
            var paymentVouchers = vouchersByPayment[paymentId].ToList();
            var withdrawalIds = paymentLinks.Select(l => l.Withdrawal_ID).Distinct().ToList();

            // Not split - every voucher on this payment unambiguously
            // belongs to the one withdrawal that funded it.
            if (withdrawalIds.Count <= 1)
            {
                foreach (var withdrawalId in withdrawalIds)
                    result[(withdrawalId, paymentId)] = paymentVouchers;
                continue;
            }

            var sortedLinks = paymentLinks.OrderBy(l => l.Allocated_Amount).ThenBy(l => l.Withdrawal_ID).ToList();
            var sortedVouchers = paymentVouchers.OrderBy(v => v.AllocatedFromThisPayment).ThenBy(v => v.Voucher_ID).ToList();

            var amountsLineUp = sortedLinks.Count == sortedVouchers.Count &&
                sortedLinks.Zip(sortedVouchers, (l, v) => l.Allocated_Amount == v.AllocatedFromThisPayment).All(match => match);

            if (!amountsLineUp)
            {
                unattributed.Add(paymentId);
                foreach (var withdrawalId in withdrawalIds)
                    result[(withdrawalId, paymentId)] = paymentVouchers;
                continue;
            }

            for (var i = 0; i < sortedLinks.Count; i++)
            {
                var key = (sortedLinks[i].Withdrawal_ID, paymentId);
                if (!result.TryGetValue(key, out var list))
                    result[key] = list = new List<WithdrawalPaymentVoucherRow>();

                list.Add(sortedVouchers[i]);
            }
        }

        return (result, unattributed);
    }

    public record WithdrawalDetailRow(
        string WithdrawalId, DateTime WithdrawalDate, decimal WithdrawalAmount, decimal WithdrawalRemaining,
        string? PaymentId, DateTime? PaymentDate, string? PaymentMode, decimal? FundedFromThisWithdrawal, decimal? PaymentTotal, bool? SplitUnattributed,
        string? VoucherId, DateTime? VoucherDate, string? Payee, string? VoucherDescription, string? BudgetId, string? BudgetCategory, decimal? AllocatedFromThisPayment);

    // Flattened one row per (Withdrawal, Payment, Voucher) so it reads
    // cleanly as a table - a withdrawal with no payments yet, or a
    // payment with no voucher attributed to it, still gets one row with
    // the deeper columns blank rather than being dropped.
    private static List<WithdrawalDetailRow> BuildDetailRows(
        IEnumerable<WithdrawalListRow> withdrawals,
        ILookup<string, WithdrawalPaymentRow> paymentsByWithdrawal,
        Dictionary<(string, string), List<WithdrawalPaymentVoucherRow>> vouchersForWithdrawalPayment,
        HashSet<string> unattributed)
    {
        var rows = new List<WithdrawalDetailRow>();

        foreach (var w in withdrawals)
        {
            var paymentsForWithdrawal = paymentsByWithdrawal[w.Withdrawal_ID].ToList();
            if (paymentsForWithdrawal.Count == 0)
            {
                rows.Add(new WithdrawalDetailRow(
                    w.Withdrawal_ID, w.Withdrawal_Date, w.Amount, w.Remaining,
                    null, null, null, null, null, null,
                    null, null, null, null, null, null, null));
                continue;
            }

            foreach (var p in paymentsForWithdrawal)
            {
                var vouchersForThisWithdrawal = vouchersForWithdrawalPayment.TryGetValue((w.Withdrawal_ID, p.Payment_ID), out var list)
                    ? list
                    : new List<WithdrawalPaymentVoucherRow>();
                var isUnattributed = unattributed.Contains(p.Payment_ID);

                if (vouchersForThisWithdrawal.Count == 0)
                {
                    rows.Add(new WithdrawalDetailRow(
                        w.Withdrawal_ID, w.Withdrawal_Date, w.Amount, w.Remaining,
                        p.Payment_ID, p.Payment_Date, p.Payment_Mode, p.FundedFromThisWithdrawal, p.Amount_Paid, isUnattributed,
                        null, null, null, null, null, null, null));
                    continue;
                }

                foreach (var v in vouchersForThisWithdrawal)
                {
                    rows.Add(new WithdrawalDetailRow(
                        w.Withdrawal_ID, w.Withdrawal_Date, w.Amount, w.Remaining,
                        p.Payment_ID, p.Payment_Date, p.Payment_Mode, p.FundedFromThisWithdrawal, p.Amount_Paid, isUnattributed,
                        v.Voucher_ID, v.VoucherDate, v.PayeeDisplay, v.Description, v.Budget_ID, v.Category_Name, v.AllocatedFromThisPayment));
                }
            }
        }

        return rows;
    }

    // Two-sheet workbook, same pattern as the Official Totals export:
    // "Summary" is exactly the withdrawal-level table shown on screen
    // (same numbers as GetWithdrawalsAsync/BankReconciliation); "Detail"
    // is every underlying Withdrawal/Payment/Voucher row that rolls up
    // into it, so a Treasurer can trace any withdrawal's remaining
    // balance back to the actual vouchers it did or didn't fund yet.
    public async Task<IActionResult> OnGetExportXlsxAsync()
    {
        var withdrawals = (await _bankingService.GetWithdrawalsAsync()).ToList();
        var paymentRows = (await _bankingService.GetWithdrawalPaymentsAsync()).ToList();
        var links = await _bankingService.GetWithdrawalPaymentLinksAsync();
        var vouchers = await _bankingService.GetWithdrawalPaymentVouchersAsync();

        var paymentsByWithdrawal = paymentRows.ToLookup(p => p.Withdrawal_ID);
        var (vouchersForWithdrawalPayment, unattributed) = AttributeVouchers(paymentRows, links, vouchers);
        var detailRows = BuildDetailRows(withdrawals, paymentsByWithdrawal, vouchersForWithdrawalPayment, unattributed);
        var budgetCategoriesByWithdrawal = BuildBudgetCategoriesByWithdrawal(withdrawals, paymentsByWithdrawal, vouchersForWithdrawalPayment);

        using var workbook = new XLWorkbook();

        var summary = workbook.Worksheets.Add("Summary");
        string[] summaryHeaders = { "Withdrawal", "Date", "Reference No", "Bank Account", "Description", "Amount", "Allocated", "Charges", "Remaining" };
        for (var i = 0; i < summaryHeaders.Length; i++)
            summary.Cell(1, i + 1).Value = summaryHeaders[i];

        var row = 2;
        foreach (var w in withdrawals)
        {
            var categories = budgetCategoriesByWithdrawal.TryGetValue(w.Withdrawal_ID, out var c) ? c : new List<string>();

            summary.Cell(row, 1).Value = w.Withdrawal_ID;
            summary.Cell(row, 2).Value = w.Withdrawal_Date;
            summary.Cell(row, 3).Value = w.Reference_No;
            summary.Cell(row, 4).Value = w.Bank_Account;
            summary.Cell(row, 5).Value = categories.Count > 0 ? string.Join(", ", categories) : "No vouchers attributed yet";
            summary.Cell(row, 6).Value = w.Amount;
            summary.Cell(row, 7).Value = w.Allocated;
            summary.Cell(row, 8).Value = w.Charges;
            summary.Cell(row, 9).Value = w.Remaining;
            row++;
        }
        summary.Cell(row, 1).Value = "Total";
        summary.Cell(row, 6).Value = withdrawals.Sum(w => w.Amount);
        summary.Cell(row, 7).Value = withdrawals.Sum(w => w.Allocated);
        summary.Cell(row, 8).Value = withdrawals.Sum(w => w.Charges);
        summary.Cell(row, 9).Value = withdrawals.Sum(w => w.Remaining);

        summary.Range(1, 1, 1, summaryHeaders.Length).Style.Font.Bold = true;
        summary.Range(row, 1, row, summaryHeaders.Length).Style.Font.Bold = true;
        summary.Column(2).Style.DateFormat.Format = "yyyy-mm-dd";
        summary.Range(2, 6, row, 9).Style.NumberFormat.Format = "#,##0.00";
        summary.Columns().AdjustToContents();

        var detail = workbook.Worksheets.Add("Detail");
        string[] detailHeaders =
        {
            "Withdrawal", "Withdrawal Date", "Withdrawal Amount", "Withdrawal Remaining",
            "Payment", "Payment Date", "Payment Mode", "Funded From This Withdrawal", "Payment Total", "Split Unattributed",
            "Voucher", "Voucher Date", "Payee", "Voucher Description", "Budget Code", "Budget Category", "Allocated From This Payment"
        };
        for (var i = 0; i < detailHeaders.Length; i++)
            detail.Cell(1, i + 1).Value = detailHeaders[i];

        var dRow = 2;
        foreach (var d in detailRows)
        {
            detail.Cell(dRow, 1).Value = d.WithdrawalId;
            detail.Cell(dRow, 2).Value = d.WithdrawalDate;
            detail.Cell(dRow, 3).Value = d.WithdrawalAmount;
            detail.Cell(dRow, 4).Value = d.WithdrawalRemaining;
            detail.Cell(dRow, 5).Value = d.PaymentId;
            if (d.PaymentDate.HasValue) detail.Cell(dRow, 6).Value = d.PaymentDate.Value;
            detail.Cell(dRow, 7).Value = d.PaymentMode;
            if (d.FundedFromThisWithdrawal.HasValue) detail.Cell(dRow, 8).Value = d.FundedFromThisWithdrawal.Value;
            if (d.PaymentTotal.HasValue) detail.Cell(dRow, 9).Value = d.PaymentTotal.Value;
            if (d.SplitUnattributed.HasValue) detail.Cell(dRow, 10).Value = d.SplitUnattributed.Value ? "Yes" : "No";
            detail.Cell(dRow, 11).Value = d.VoucherId;
            if (d.VoucherDate.HasValue) detail.Cell(dRow, 12).Value = d.VoucherDate.Value;
            detail.Cell(dRow, 13).Value = d.Payee;
            detail.Cell(dRow, 14).Value = d.VoucherDescription;
            detail.Cell(dRow, 15).Value = d.BudgetId;
            detail.Cell(dRow, 16).Value = d.BudgetCategory;
            if (d.AllocatedFromThisPayment.HasValue) detail.Cell(dRow, 17).Value = d.AllocatedFromThisPayment.Value;
            dRow++;
        }

        detail.Range(1, 1, 1, detailHeaders.Length).Style.Font.Bold = true;
        detail.Column(2).Style.DateFormat.Format = "yyyy-mm-dd";
        detail.Column(6).Style.DateFormat.Format = "yyyy-mm-dd";
        detail.Column(12).Style.DateFormat.Format = "yyyy-mm-dd";
        detail.Range(2, 3, dRow - 1, 4).Style.NumberFormat.Format = "#,##0.00";
        detail.Range(2, 8, dRow - 1, 9).Style.NumberFormat.Format = "#,##0.00";
        detail.Column(17).Style.NumberFormat.Format = "#,##0.00";
        detail.Columns().AdjustToContents();

        // AutoFilter, not a real PivotTable - see Official Totals export
        // for the same reasoning: a clean source range is enough for
        // anyone who wants to build their own live pivot in Excel.
        if (dRow > 2)
            detail.Range(1, 1, dRow - 1, detailHeaders.Length).SetAutoFilter();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return new FileContentResult(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
        {
            FileDownloadName = "WithdrawalBreakdown.xlsx"
        };
    }
}
