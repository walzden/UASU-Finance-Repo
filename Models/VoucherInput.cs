using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace UASU_VoucherApprovals.Models;

// Form input for creating a new voucher (the "populate the database"
// front end). Mirrors the CK_Voucher_PayeeCategory constraint in the
// database: exactly one of Official/Supplier/manual name must be set
// depending on Payee_Category, enforced again here so the user gets a
// friendly validation message instead of a raw SQL error.
public class VoucherInputModel
{
    // Vouchers.VoucherDate defaults to GETDATE() in the database, but
    // that's only right for a voucher entered the same day something
    // happened - income received into the account (or an expense) that
    // wasn't recorded until later needs to reflect the actual period it
    // belongs to, not the day someone got around to typing it in.
    [Required]
    public DateTime VoucherDate { get; set; } = DateTime.Today;

    [Required]
    public string Transaction_Type { get; set; } = "Expense"; // Income | Expense

    [Required]
    public string Budget_Link { get; set; } = string.Empty;

    [Required]
    public string Payee_Category { get; set; } = "One-Time"; // Official | Supplier | One-Time | Donor | Income Source

    public string? Official_Link { get; set; }
    public string? Supplier_Link { get; set; }
    public string? Manual_Payee_Name { get; set; }

    // Required (for Expense payments) when the payee is a one-time payee
    // with no reference row to carry an address of their own - see
    // Create.cshtml.cs's NormalizePayeeFields and
    // CK_Vouchers_ManualPayeeAddress. Labour Relations (Accounts)
    // Regulations reg. 6(1)(e): every payment voucher must record the
    // name AND address of the recipient.
    public string? Manual_Payee_Address { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
    public decimal Amount { get; set; }

    [MaxLength(255)]
    public string? Description { get; set; }

    // When set, this voucher is a payment toward an existing debt.
    // CreateVoucherAsync looks up the debt and forces Payee_Category /
    // Official_Link / Supplier_Link / Transaction_Type to match it
    // server-side, so the payee fields above can't drift out of sync
    // with who the debt is actually owed to.
    public string? Debt_Link { get; set; }

    // Reg. 6(1)(d): required whenever this payment is for travelling
    // expenses, regardless of payee or budget category - see
    // NormalizeTravelFields and CK_Vouchers_TravelDetails.
    public bool Is_Travel_Expense { get; set; }
    public string? Traveler_Name { get; set; }
    public string? Travel_From { get; set; }
    public string? Travel_To { get; set; }
    public string? Travel_Mode { get; set; }
    public DateTime? Travel_Date { get; set; }
    public string? Travel_Reason { get; set; }

    // When set, this voucher is a corrected resubmission of an earlier
    // rejected voucher (see GetRejectedVouchersAwaitingCorrectionAsync).
    // The old voucher is left exactly as it was submitted - never
    // edited - this just links the two for traceability.
    public string? Supersedes_Voucher_ID { get; set; }

    // Income-only: most income is recorded after the fact, once the
    // money has already landed (bank deposit, M-Pesa, cash) - Mark_As_
    // Received lets CreateVoucherAsync record the matching Payment in
    // the same submission instead of a separate trip to Record Payment.
    // Defaults true since that's the normal case; unticking it leaves
    // the voucher exactly as before (payable later, same as any other
    // payment-eligible voucher).
    public bool Mark_As_Received { get; set; } = true;
    public string? Payment_Mode { get; set; } // M-PESA | Cheque | Cash | Bank Transfer
    public string? Payment_Reference_No { get; set; }
    public string? Payment_Bank_Account { get; set; }
}

// A rejected voucher with no corrected replacement yet - shown on
// Vouchers/Create both as the "what needs fixing" list and as the
// source for the supersession picker.
public class RejectedVoucherRow
{
    public string Voucher_ID { get; set; } = string.Empty;
    public DateTime VoucherDate { get; set; }
    public decimal Amount { get; set; }
    public string? Description { get; set; }
    public string PayeeDisplay { get; set; } = string.Empty;
    public string RejectedByRole { get; set; } = string.Empty;
    public string? RejectionReason { get; set; }
    public DateTime RejectionDate { get; set; }
}

public class SimpleOption
{
    public string Id { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
}

// One voucher still awaiting approval, with its full editable field set
// - backs Pages/Vouchers/Pending.cshtml, both the list row and the
// inline correction form (pre-filled from these same values). CanEdit
// is false once either approver has recorded any decision (approve or
// reject) - trg_Vouchers_BlockEditAfterApproval enforces the same rule
// at the database layer regardless of what this flag says, so this is
// purely about what the page offers, not the actual safety boundary.
public class PendingVoucherRow
{
    public string Voucher_ID { get; set; } = string.Empty;
    public DateTime VoucherDate { get; set; }
    public string Transaction_Type { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Description { get; set; }
    public string PayeeDisplay { get; set; } = string.Empty;

    public string Payee_Category { get; set; } = string.Empty;
    public string? Official_Link { get; set; }
    public string? Supplier_Link { get; set; }
    public string? Manual_Payee_Name { get; set; }
    public string? Manual_Payee_Address { get; set; }
    public string Budget_Link { get; set; } = string.Empty;
    public string? Debt_Link { get; set; }

    public bool Is_Travel_Expense { get; set; }
    public string? Traveler_Name { get; set; }
    public string? Travel_From { get; set; }
    public string? Travel_To { get; set; }
    public string? Travel_Mode { get; set; }
    public DateTime? Travel_Date { get; set; }
    public string? Travel_Reason { get; set; }

    public bool ChairmanApproved { get; set; }
    public bool ChapterSecretaryApproved { get; set; }

    public bool CanEdit => !ChairmanApproved && !ChapterSecretaryApproved;
}

// One voucher that has cleared approval (or is Is_Legacy, pre-dating
// the workflow) - the list row for Pages/Reports/ApprovedVouchers.cshtml.
public class ApprovedVoucherRow
{
    public string Voucher_ID { get; set; } = string.Empty;
    public DateTime VoucherDate { get; set; }
    public string Transaction_Type { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Description { get; set; }
    public string PayeeDisplay { get; set; } = string.Empty;
    public string Payee_Category { get; set; } = string.Empty;
    public string? Budget_Category { get; set; }
    public bool Is_Travel_Expense { get; set; }
    public string? Traveler_Name { get; set; }
    public string? Travel_From { get; set; }
    public string? Travel_To { get; set; }
    public string? Travel_Mode { get; set; }
    public DateTime? Travel_Date { get; set; }
    public string? Travel_Reason { get; set; }
    public bool Is_Legacy { get; set; }
    public string Current_Status { get; set; } = string.Empty;
}

// One approval/rejection decision on a voucher shown on the Approved
// Vouchers detail panel - a voucher can have up to two (Chairman,
// Chapter Secretary), or none at all if Is_Legacy.
public class VoucherApprovalActionRow
{
    public string Voucher_ID { get; set; } = string.Empty;
    public string ApproverRole { get; set; } = string.Empty;
    public string ApproverName { get; set; } = string.Empty;
    public string Approval_Status { get; set; } = string.Empty;
    public DateTime Approval_Date { get; set; }
    public string? Comments { get; set; }
}

// One payment allocation touching a voucher, for the same detail panel
// - almost always one row, but a voucher's Amount can in principle be
// split across more than one payment.
public class VoucherPaymentRow
{
    public string Voucher_ID { get; set; } = string.Empty;
    public string Payment_ID { get; set; } = string.Empty;
    public DateTime Payment_Date { get; set; }
    public string Payment_Mode { get; set; } = string.Empty;
    public decimal Allocated_Amount { get; set; }
}

// Same shape as SimpleOption, plus the plain FullName - Label is
// "FullName (Role)" for the dropdown, but Create.cshtml needs the bare
// name on its own to prefill Traveler_Name when this official is
// selected as the payee.
// A fully-approved, unpaid voucher shown on Payments/Record so the
// Treasurer can browse what's ready to pay before picking one, rather
// than reading terse labels off a bare dropdown.
public class ApprovedUnpaidVoucherRow
{
    public string Voucher_ID { get; set; } = string.Empty;
    public DateTime VoucherDate { get; set; }
    public string Transaction_Type { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Description { get; set; }
    public string PayeeDisplay { get; set; } = string.Empty;
    public bool Is_Legacy { get; set; }
}

public class OfficialOption
{
    public string Id { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
}

public class PaymentInputModel
{
    // One or more approved, unpaid vouchers to settle together with this
    // single payment - each is allocated in full (its own Amount), so
    // Amount_Paid below is expected to equal their sum. Checking exactly
    // one reproduces the old single-voucher behavior.
    [Required]
    [MinLength(1, ErrorMessage = "Select at least one voucher.")]
    public List<string> Voucher_IDs { get; set; } = new();

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
    public decimal Amount_Paid { get; set; }

    [Required]
    public string Payment_Mode { get; set; } = "Bank Transfer"; // M-PESA | Cheque | Cash | Bank Transfer

    public string? Reference_No { get; set; }
    public string? Bank_Account { get; set; }
    public string? Description { get; set; }

    // When set, this payment was funded from cash drawn via this
    // withdrawal - RecordPaymentAsync also inserts the WithdrawalPayments
    // link row so the two stay tied together.
    public string? Withdrawal_Link { get; set; }
}

public class AcknowledgementInputModel
{
    [Required]
    public string Payment_ID { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    public string Acknowledged_By { get; set; } = string.Empty;

    [Required]
    public string Method { get; set; } = "Signed Voucher";

    public string? Notes { get; set; }

    // Optional evidence, e.g. a screenshot of a WhatsApp confirmation
    // message. Validated server-side in VoucherService (size + actual
    // file signature, not just the browser-supplied Content-Type)
    // before it's ever written to Payment_Acknowledgements.
    public IFormFile? Attachment { get; set; }
}

public class PaymentAcknowledgementOption
{
    public string Id { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;

    // Null when the payment's allocations span more than one distinct
    // payee (e.g. two vouchers for different people combined into one
    // payment) - there's no single correct name to prefill in that case,
    // so the form is left for the user to fill in by hand as before.
    public string? PayeeName { get; set; }
}

public class AcknowledgementRow
{
    public string Payment_ID { get; set; } = string.Empty;
    public DateTime Acknowledgement_Date { get; set; }
    public string Acknowledged_By { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public bool HasAttachment { get; set; }
}

public class AcknowledgementAttachment
{
    public byte[] Attachment_Data { get; set; } = Array.Empty<byte>();
    public string Attachment_FileName { get; set; } = string.Empty;
    public string Attachment_ContentType { get; set; } = string.Empty;
}

public class ChangePasswordInputModel
{
    [Required]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required]
    [MinLength(8, ErrorMessage = "New password must be at least 8 characters.")]
    public string NewPassword { get; set; } = string.Empty;

    [Required]
    [Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
