namespace UASU_VoucherApprovals.Models;

// One row in the "vouchers awaiting my approval" list.
public class PendingApproval
{
    public string Voucher_ID { get; set; } = string.Empty;
    public DateTime VoucherDate { get; set; }
    public string Transaction_Type { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Description { get; set; }
    public string PayeeDisplay { get; set; } = string.Empty;
    public bool ChairmanApproved { get; set; }
    public bool ChapterSecretaryApproved { get; set; }
    public bool ChairmanRejected { get; set; }
    public bool ChapterSecretaryRejected { get; set; }

    // Backs the "bulk approve" grouping on Approvals/Index - vouchers on
    // the same Budget_ID whose VoucherDate falls in the same Monday-
    // starting week get bundled into one group with a single "Approve
    // all" action. WeekStart comes straight from the query (the Monday
    // of VoucherDate's week, computed with a DATEDIFF/7 trick that's
    // independent of the server's @@DATEFIRST/locale setting) so the
    // grouping key the page uses can never drift from what SQL computed.
    public string? Budget_ID { get; set; }
    public string? Budget_Category { get; set; }
    public DateTime WeekStart { get; set; }

    // Set when this voucher is a corrected resubmission of an earlier
    // rejected one, so the approver reviewing it has the context of
    // what was wrong last time rather than voting blind.
    public string? Supersedes_Voucher_ID { get; set; }
    public string? SupersededRejectionReason { get; set; }

    // The payee's own role, and whether the payee is the approver looking
    // at this voucher. An approver may still approve a voucher payable to
    // themselves (the other role must sign off separately), but the page
    // flags it so it is never approved unknowingly.
    public string? PayeeRole { get; set; }
    public bool IsOwnVoucher { get; set; }
}

// Payload posted from the Approve/Reject buttons.
public class ApprovalDecision
{
    public string VoucherID { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // 'Approved' or 'Rejected'
    public string? Comments { get; set; }
}

// One voucher's outcome from a bulk "Approve all" click - a voucher
// that fails (already ruled on by someone else in the moment between
// page load and submit, say) doesn't stop the rest of the group, so
// the page needs a per-voucher result to report back, not just one
// overall success/failure.
public class BulkApprovalResult
{
    public string Voucher_ID { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? Message { get; set; }
}
