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
