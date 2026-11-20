using System.ComponentModel.DataAnnotations;

namespace UASU_VoucherApprovals.Models;

// New Debt: either one supplier, or one or more officials ticked from a
// list. Ticking several records a common (group) debt - each official
// still gets their own Debt_Register row for their share (CK_Debt_
// CreditorType allows one creditor per debt), all sharing the
// description, invoice and dates, created together.
public class DebtEntryInputModel
{
    [Required]
    public string Creditor_Type { get; set; } = "Official"; // Official | Supplier

    // Supplier debt.
    public string? Supplier_link { get; set; }
    public decimal? Supplier_Amount { get; set; }

    // Official debt(s): one row per listed official.
    public List<DebtOfficialLine> Officials { get; set; } = new();

    [Required(ErrorMessage = "Enter the date (or first day) the debt was incurred.")]
    public DateTime Date_Incurred { get; set; } = DateTime.Today;

    // Set only when the debt spans several days (SQL/038).
    public bool Spans_Days { get; set; }
    public DateTime? Date_Incurred_To { get; set; }

    [MaxLength(50)]
    public string? Invoice_No { get; set; }

    [Required(ErrorMessage = "Describe what the debt is for.")]
    [MaxLength(255)]
    public string? Description { get; set; }
}

public class DebtOfficialLine
{
    public string OfficialID { get; set; } = string.Empty;
    public bool Selected { get; set; }
    public decimal? Amount { get; set; }
}

// One debt to create, after the page has checked the form.
public record NewDebt(string Creditor_Type, string? Official_link, string? Supplier_link, decimal Total_Owed);

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

    // Last day when the debt spans several days (SQL/038); null for one day.
    public DateTime? Date_Incurred_To { get; set; }

    // "1 Oct 2026", or a range: "1–3 Oct 2026", "30 Sep – 2 Oct 2026",
    // "30 Dec 2026 – 2 Jan 2027".
    public string DateLabel => FormatDateRange(Date_Incurred, Date_Incurred_To);

    public static string FormatDateRange(DateTime from, DateTime? to)
    {
        if (to is null || to.Value.Date == from.Date) return from.ToString("d MMM yyyy");
        var t = to.Value;
        if (from.Year != t.Year) return $"{from:d MMM yyyy} – {t:d MMM yyyy}";
        if (from.Month != t.Month) return $"{from:d MMM} – {t:d MMM yyyy}";
        return $"{from:%d}–{t:d MMM yyyy}";
    }

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
