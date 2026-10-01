using System.ComponentModel.DataAnnotations;

namespace UASU_VoucherApprovals.Models;

// One current official on the Bulk Vouchers page. HasVoucher is true when
// a non-rejected voucher with the same description already exists for
// them - the page shows it, and CreateAsync skips it, so submitting the
// same batch twice can't pay anyone twice.
public class BulkVoucherCandidateRow
{
    public string OfficialID { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Role { get; set; }
    public bool HasVoucher { get; set; }
}

// One ticked row coming back from the page.
public class BulkVoucherLine
{
    public string OfficialID { get; set; } = string.Empty;
    public bool Selected { get; set; }
    public decimal? Amount { get; set; }
}

public class BulkVoucherInputModel
{
    [Required]
    public DateTime VoucherDate { get; set; } = DateTime.Today;

    [Required(ErrorMessage = "Choose a budget code for these vouchers.")]
    public string Budget_Link { get; set; } = string.Empty;

    // Also the duplicate guard: one official can only have one
    // (non-rejected) voucher per description, so make it specific,
    // e.g. "October 2026 AGM sitting allowance".
    [Required(ErrorMessage = "Enter a description for these vouchers.")]
    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    // Pre-fills every row's amount on the page; each row can still be
    // changed individually before submitting.
    [Range(0.01, 10000000, ErrorMessage = "Amount must be greater than zero.")]
    public decimal? DefaultAmount { get; set; }

    public List<BulkVoucherLine> Lines { get; set; } = new();
}
