namespace UASU_VoucherApprovals.Models;

// One line of a traditional two-column (Cash/Bank) cash book ledger -
// mirrors the physical UASU Form V cash book: Date, Item, Voucher/
// Receipt No., Money Received (Cash/Bank), Money Paid (Cash/Bank).
// RunningCashBalance/RunningBankBalance are computed in C# across the
// full unfiltered history and then the list is filtered to the
// requested year for display - see ReportService.GetCashBookAsync -
// so a balance shown for any visible row is still correct even though
// only one year's rows are on screen at a time.
public class CashBookRow
{
    public DateTime EntryDate { get; set; }
    public string Item { get; set; } = string.Empty;
    public string? VoucherOrReceiptNo { get; set; }
    public decimal ReceivedCash { get; set; }
    public decimal ReceivedBank { get; set; }
    public decimal PaidCash { get; set; }
    public decimal PaidBank { get; set; }

    public decimal RunningCashBalance { get; set; }
    public decimal RunningBankBalance { get; set; }
}
