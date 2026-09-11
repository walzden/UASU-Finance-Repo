using System.ComponentModel.DataAnnotations;

namespace UASU_VoucherApprovals.Models;

public class BankWithdrawalInputModel
{
    [Required]
    public DateTime Withdrawal_Date { get; set; } = DateTime.Today;

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
    public decimal Amount { get; set; }

    public string? Reference_No { get; set; }
    public string? Bank_Account { get; set; }
    public string? Notes { get; set; }
}

// One row of the withdrawals list, with how much of it has already
// been tied to payments and how much is still unallocated.
public class WithdrawalListRow
{
    public string Withdrawal_ID { get; set; } = string.Empty;
    public DateTime Withdrawal_Date { get; set; }
    public decimal Amount { get; set; }
    public decimal Allocated { get; set; }
    public decimal Charges { get; set; }
    public decimal Remaining { get; set; }
    public string? Reference_No { get; set; }
    public string? Bank_Account { get; set; }
    public string? Notes { get; set; }
}

// One payment funded, in whole or in part, from a given withdrawal -
// FundedFromThisWithdrawal is WithdrawalPayments.Allocated_Amount, not
// the payment's own total (Amount_Paid), since a payment can be split
// across more than one withdrawal. WithdrawalCount > 1 flags exactly
// that case so the page can say so rather than implying this one
// withdrawal covers the whole payment.
public class WithdrawalPaymentRow
{
    public string Withdrawal_ID { get; set; } = string.Empty;
    public string Payment_ID { get; set; } = string.Empty;
    public DateTime Payment_Date { get; set; }
    public string Payment_Mode { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal FundedFromThisWithdrawal { get; set; }
    public decimal Amount_Paid { get; set; }
    public int WithdrawalCount { get; set; }
}

// One raw, ungrouped WithdrawalPayments row - unlike WithdrawalPaymentRow
// (which SUMs these per Withdrawal/Payment pair for display), this is
// used only to attribute individual vouchers to the specific withdrawal
// that funded them, by matching amounts against PaymentAllocations.
public class WithdrawalPaymentLinkRow
{
    public string Withdrawal_ID { get; set; } = string.Empty;
    public string Payment_ID { get; set; } = string.Empty;
    public decimal Allocated_Amount { get; set; }
}

// One voucher a payment was allocated against, for the withdrawal
// breakdown report's innermost level.
public class WithdrawalPaymentVoucherRow
{
    public string Payment_ID { get; set; } = string.Empty;
    public string Voucher_ID { get; set; } = string.Empty;
    public DateTime VoucherDate { get; set; }
    public string? PayeeDisplay { get; set; }
    public string? Description { get; set; }
    public decimal VoucherAmount { get; set; }
    public decimal AllocatedFromThisPayment { get; set; }

    // Null when the voucher has no Budget_Link set - not every voucher
    // is coded to a budget category.
    public string? Budget_ID { get; set; }
    public string? Category_Name { get; set; }

    // Vouchers paid before this approval workflow existed (SQL/002)
    // have no Voucher_Approvals rows at all - that's expected, not a
    // sign they're still awaiting approval, so callers need this to
    // tell the two cases apart.
    public bool Is_Legacy { get; set; }
}

public class PaymentChargeInputModel
{
    [Required]
    public string Payment_ID { get; set; } = string.Empty;

    // Optional - only set when this charge came from cash drawn via a
    // specific withdrawal. trg_PaymentCharges_ValidateWithdrawal in the
    // database rejects this unless WithdrawalPayments already links
    // this exact Withdrawal_ID to this exact Payment_ID.
    public string? Withdrawal_ID { get; set; }

    [Required]
    public string Charge_Type { get; set; } = "Bank Fee"; // Bank Fee | M-PESA Fee | Other - CK_PaymentCharges_ChargeType allows exactly these three

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
    public decimal Charge_Amount { get; set; }

    public string? Notes { get; set; }
}

// Distinct from PaymentChargeInputModel - this is a charge the bank
// deducted directly from the account (ledger fee, excise duty,
// maintenance), not caused by any specific payment or withdrawal, so
// there's no Payment_ID/Withdrawal_ID to attach it to.
public class AccountChargeInputModel
{
    [Required]
    public DateTime Charge_Date { get; set; } = DateTime.Today;

    [Required]
    public string Bank_Account { get; set; } = string.Empty;

    [Required]
    public string Charge_Type { get; set; } = "Account Ledger Fee"; // Account Ledger Fee | Excise Charges | Maintenance Fee | Other

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
    public decimal Charge_Amount { get; set; }

    public string? Notes { get; set; }
}

public class AccountChargeRow
{
    public string Charge_ID { get; set; } = string.Empty;
    public DateTime Charge_Date { get; set; }
    public string Bank_Account { get; set; } = string.Empty;
    public string Charge_Type { get; set; } = string.Empty;
    public decimal Charge_Amount { get; set; }
    public string? Notes { get; set; }
}

// Adds one more voucher to a payment that already exists - this is
// what makes "one payment covers several vouchers" actually usable
// from the UI, without reworking the main Record Payment form that
// already covers the common single-voucher case.
public class AddAllocationInputModel
{
    [Required]
    public string Payment_ID { get; set; } = string.Empty;

    [Required]
    public string Voucher_ID { get; set; } = string.Empty;

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
    public decimal Allocated_Amount { get; set; }
}
