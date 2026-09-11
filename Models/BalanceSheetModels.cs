namespace UASU_VoucherApprovals.Models;

// "Statement of Financial Position" as at a given month-end - the two
// asset lines reuse CertificationService.GetBookBalancesAsync's book
// balances (already computed on the same cumulative-to-date basis used
// for monthly certification), so this can never drift from what that
// report already says the cash/bank position was. Accounts Payable is
// the one line this report computes itself, since DebtSummary is a
// live "as of right now" view with no cutoff date of its own - see
// ReportService.GetBalanceSheetAsync.
public class BalanceSheetData
{
    public DateTime AsOfDate { get; set; }

    public decimal CashInHand { get; set; }
    public decimal BankBalance { get; set; }
    public decimal TotalAssets => CashInHand + BankBalance;

    public decimal AccountsPayable { get; set; }
    public decimal TotalLiabilities => AccountsPayable;

    // Residual, not a separately-tracked ledger balance - standard for
    // a cash-basis, no-fixed-assets organization like a union chapter.
    public decimal NetAssets => TotalAssets - TotalLiabilities;
}
