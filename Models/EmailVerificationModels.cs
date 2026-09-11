using System.ComponentModel.DataAnnotations;

namespace UASU_VoucherApprovals.Models;

public class VerifyEmailInputModel
{
    [Required]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "Enter the 6-digit code.")]
    public string Code { get; set; } = string.Empty;
}

// Just the two columns needed to check a submitted code against what
// was issued - kept separate from OfficialUser/UserCredentialRecord
// since those never need to carry the hash past the auth check either.
public class VerificationCodeRecord
{
    public string? VerificationCodeHash { get; set; }
    public DateTime? VerificationCodeExpiry { get; set; }
}
