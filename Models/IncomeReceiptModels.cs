namespace UASU_VoucherApprovals.Models;

// One (payment, voucher) pair behind an income receipt - flattened, same
// pattern as WithdrawalPaymentVoucherRow, grouped by Payment_ID in
// FormQModel to build one receipt per payment. Category_Name drives
// which of Form Q's four fixed "Being payment of" rows (Monthly
// Subscription / Entrance Fees / Donations / Others) the amount falls
// into - see FormQModel.BucketFor.
public class IncomeReceiptVoucherRow
{
    public string Payment_ID { get; set; } = string.Empty;
    public DateTime Payment_Date { get; set; }
    public string Payment_Mode { get; set; } = string.Empty;
    public string? Reference_No { get; set; }
    public string? Bank_Account { get; set; }
    public string? PayerName { get; set; }

    public string Voucher_ID { get; set; } = string.Empty;
    public string? Category_Name { get; set; }
    public decimal AllocatedFromThisPayment { get; set; }
}
