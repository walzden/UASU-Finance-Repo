using System.ComponentModel.DataAnnotations;

namespace UASU_VoucherApprovals.Models;

public class ForgotPasswordInputModel
{
    [Required]
    [MaxLength(100)]
    public string Username { get; set; } = string.Empty;
}

public class ResetPasswordInputModel
{
    [Required]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "Enter the 6-digit code.")]
    public string Code { get; set; } = string.Empty;

    [Required]
    [MinLength(8, ErrorMessage = "New password must be at least 8 characters.")]
    public string NewPassword { get; set; } = string.Empty;

    [Required]
    [Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
