using System.ComponentModel.DataAnnotations;

namespace UASU_VoucherApprovals.Models;

// Mirrors CK_Debt_CreditorType: exactly one of Official_link/Supplier_link
// must be set, matching Creditor_Type.
public class DebtInputModel
{
    [Required]
    public string Creditor_Type { get; set; } = "Supplier"; // Official | Supplier

    public string? Official_link { get; set; }
    public string? Supplier_link { get; set; }

    [Required]
    public DateTime Date_Incurred { get; set; } = DateTime.Today;

    public string? Invoice_No { get; set; }

    [MaxLength(255)]
    public string? Description { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
    public decimal Total_Owed { get; set; }
}

// Same shape as SimpleOption, plus the debt's own Description - the
// dropdown label already shows creditor/balance, but Vouchers/Create
// needs the bare description on its own to prefill Input.Description
// when this debt is selected.
public class DebtOption
{
    public string Id { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string? Description { get; set; }

    // What's owed less what live vouchers already cover (SQL/035) - the
    // most a new voucher for this debt may be for.
    public decimal Remaining { get; set; }
}

// One row of the debt status/aging report - built from DebtSummary
// (which already computes TotalAllocated/Balance/SettlementStatus)
// joined to the creditor's name. Aging is computed in C#, not SQL.
public class DebtStatusRow
{
    public string Debt_ID { get; set; } = string.Empty;
    public string Creditor_Type { get; set; } = string.Empty;
    public string? CreditorName { get; set; }
    public DateTime Date_Incurred { get; set; }
    public string? Invoice_No { get; set; }
    public string? Description { get; set; }
    public decimal Total_Owed { get; set; }
    public decimal TotalAllocated { get; set; }
    public decimal Balance { get; set; }
    public string SettlementStatus { get; set; } = string.Empty;

    // On live (not rejected, not voided) vouchers, paid or not -
    // fn_DebtCommitments (SQL/035).
    public decimal Committed { get; set; }

    // Raised on a voucher but not yet paid: the voucher is still awaiting
    // approval, or approved and waiting to be paid.
    public decimal AwaitingPayment => Math.Max(0, Committed - TotalAllocated);

    // Not on any voucher yet - what New Voucher / Pay Debts can still raise.
    public decimal StillToRaise => Math.Max(0, Total_Owed - Committed);

    public int DaysOutstanding => (DateTime.Today - Date_Incurred).Days;

    public string AgingBucket => SettlementStatus == "Settled"
        ? "Settled"
        : DaysOutstanding switch
        {
            <= 30 => "0-30 days",
            <= 60 => "31-60 days",
            <= 90 => "61-90 days",
            _ => "90+ days"
        };
}

// One voucher raised against a debt, for the debt detail page - linked
// either directly (Vouchers.Debt_Link) or through Pay Debts
// (VoucherDebts, possibly alongside other debts of the same official).
public class DebtVoucherRow
{
    public string Voucher_ID { get; set; } = string.Empty;
    public DateTime VoucherDate { get; set; }
    public string? Description { get; set; }
    public decimal VoucherAmount { get; set; }

    // This debt's part of the voucher: the whole voucher for a direct
    // link, or this debt's own line for a Pay Debts voucher.
    public decimal DebtShare { get; set; }
    public bool SharedWithOtherDebts { get; set; }

    // Voided | Rejected | Paid | Partially Paid | Approved - not paid | Awaiting approval
    public string Status { get; set; } = string.Empty;
    public string? StatusNote { get; set; }
    public decimal PaidOnVoucher { get; set; }

    // Live vouchers count towards the debt; rejected/voided ones are
    // shown for the record only.
    public bool IsLive => Status is not ("Voided" or "Rejected");
}

// One payment against a voucher on the debt.
public class DebtPaymentRow
{
    public string Voucher_ID { get; set; } = string.Empty;
    public string Payment_ID { get; set; } = string.Empty;
    public DateTime Payment_Date { get; set; }
    public string Payment_Mode { get; set; } = string.Empty;
    public string? Reference_No { get; set; }
    public decimal Allocated_Amount { get; set; }
}

public class DebtDetail
{
    public DebtStatusRow Debt { get; set; } = new();
    public List<DebtVoucherRow> Vouchers { get; set; } = new();
    public List<DebtPaymentRow> Payments { get; set; } = new();
}
