using System.ComponentModel.DataAnnotations;

namespace UASU_VoucherApprovals.Models;

public class HonorariaRateRow
{
    public string Role { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public class HonorariaRateInputModel
{
    [Required]
    [MaxLength(100)]
    public string Role { get; set; } = string.Empty;

    [Required]
    [Range(0.01, 10000000, ErrorMessage = "Amount must be greater than zero.")]
    public decimal Amount { get; set; }
}

// One current official on the Generate page's preview. Status is what
// GenerateAsync will do with them: Ready (a voucher will be created),
// AlreadyGenerated (a non-rejected voucher with the same description
// already exists for them), or NoRate (their Role has no row in
// Ref_HonorariaRates).
public class HonorariaPreviewRow
{
    public string OfficialID { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Role { get; set; }
    public decimal? Amount { get; set; }
    public string Status { get; set; } = string.Empty;
}
