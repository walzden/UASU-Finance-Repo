using System.ComponentModel.DataAnnotations;

namespace UASU_VoucherApprovals.Models;

// Cash handed to one official to spend on a purpose and account for with
// receipts (SQL/032). The float itself never touches the books; its
// receipts reach them only once retired on an approved voucher and settled.
public class CashFloatInputModel
{
    [Required]
    public string Float_Type { get; set; } = "Project"; // Project | Petty Cash | Strike Fund | Other - CK_CashFloats_Type

    [Required]
    [MaxLength(300)]
    public string Purpose { get; set; } = string.Empty;

    [Required(ErrorMessage = "Choose who is holding the cash.")]
    public string Custodian_ID { get; set; } = string.Empty;

    // Optional - blank when the float came out of cash the treasury
    // already held rather than a withdrawal made for it.
    public string? Withdrawal_ID { get; set; }

    [Required]
    public DateTime Issue_Date { get; set; } = DateTime.Today;

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
    public decimal Amount { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }
}

// One float with its position from fn_CashFloatPosition - see SQL/032 for
// what each figure means.
public class CashFloatRow
{
    public string Float_ID { get; set; } = string.Empty;
    public string Float_Type { get; set; } = string.Empty;
    public string Purpose { get; set; } = string.Empty;
    public string Custodian_ID { get; set; } = string.Empty;
    public string CustodianName { get; set; } = string.Empty;
    public string? Withdrawal_ID { get; set; }
    public DateTime Issue_Date { get; set; }
    public DateTime? Closed_Date { get; set; }
    public string? Notes { get; set; }

    public decimal Issued { get; set; }
    public decimal Spent { get; set; }
    public decimal Settled { get; set; }
    public decimal AwaitingVoucher { get; set; }
    public decimal Returned { get; set; }
    public decimal CashWithCustodian { get; set; }
    public decimal BookOutstanding { get; set; }

    public bool IsClosed => Closed_Date.HasValue;

    // Receipts on a live retirement voucher that hasn't been paid yet.
    public decimal InRetirement => Spent - Settled - AwaitingVoucher;

    public bool CanClose => !IsClosed && CashWithCustodian == 0 && BookOutstanding == 0;
}

public class CashFloatReceiptInputModel
{
    [Required]
    public string Float_ID { get; set; } = string.Empty;

    [Required]
    public DateTime Receipt_Date { get; set; } = DateTime.Today;

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
    public decimal Amount { get; set; }

    [Required(ErrorMessage = "Enter who was paid.")]
    [MaxLength(150)]
    public string Vendor { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? Receipt_No { get; set; }

    [Required]
    [MaxLength(300)]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Choose a budget code.")]
    public string Budget_Link { get; set; } = string.Empty;

    // Optional scan/photo - checked by file signature (JPEG, PNG or PDF)
    // in CashFloatService before it is stored.
    public IFormFile? Attachment { get; set; }
}

public class CashFloatReceiptRow
{
    public int Receipt_Line_ID { get; set; }
    public string Float_ID { get; set; } = string.Empty;
    public DateTime Receipt_Date { get; set; }
    public decimal Amount { get; set; }
    public string Vendor { get; set; } = string.Empty;
    public string? Receipt_No { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Budget_Link { get; set; } = string.Empty;
    public string? Category_Name { get; set; }
    public string? Voucher_ID { get; set; }
    public bool HasAttachment { get; set; }

    // Not retired | Awaiting approval | Approved - not settled | Settled | Rejected
    public string RetirementState { get; set; } = string.Empty;

    // Free to be retired: never retired, or its voucher was rejected.
    public bool CanRetire => RetirementState is "Not retired" or "Rejected";
}

public class CashFloatReturnInputModel
{
    [Required]
    public string Float_ID { get; set; } = string.Empty;

    [Required]
    public DateTime Return_Date { get; set; } = DateTime.Today;

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
    public decimal Amount { get; set; }

    [MaxLength(300)]
    public string? Notes { get; set; }
}

public class CashFloatReturnRow
{
    public int Return_ID { get; set; }
    public DateTime Return_Date { get; set; }
    public decimal Amount { get; set; }
    public string? Notes { get; set; }
    public string RecordedByName { get; set; } = string.Empty;
}

// A retirement voucher of one float, for the settle step.
public class CashFloatVoucherRow
{
    public string Voucher_ID { get; set; } = string.Empty;
    public DateTime VoucherDate { get; set; }
    public decimal Amount { get; set; }
    public string? Category_Name { get; set; }
    public int ReceiptCount { get; set; }
    public string RetirementState { get; set; } = string.Empty;
    public string? Payment_ID { get; set; }
}

public class CashFloatAttachment
{
    public byte[] Attachment_Data { get; set; } = Array.Empty<byte>();
    public string Attachment_FileName { get; set; } = string.Empty;
    public string Attachment_ContentType { get; set; } = string.Empty;
}
