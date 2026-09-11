using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UASU_VoucherApprovals.Models;
using UASU_VoucherApprovals.Services;

namespace UASU_VoucherApprovals.Pages.Account;

// No [Authorize] - reached before signing in, from ForgotPassword.
// Never reveals whether Username actually matched an account: the page
// behaves identically either way (same message, same "code sent"
// framing), so it can't be used to enumerate valid usernames.
public class ResetPasswordModel : PageModel
{
    private readonly IAuthService _authService;
    private readonly IEmailService _emailService;
    private readonly IWebHostEnvironment _env;

    public ResetPasswordModel(IAuthService authService, IEmailService emailService, IWebHostEnvironment env)
    {
        _authService = authService;
        _emailService = emailService;
        _env = env;
    }

    [BindProperty(SupportsGet = true)]
    public string Username { get; set; } = string.Empty;

    [BindProperty]
    public ResetPasswordInputModel Input { get; set; } = new();

    public string MaskedEmail { get; set; } = string.Empty;
    public string? StatusMessage { get; set; }
    public bool StatusIsError { get; set; }

    // Same dev-only fallback as VerifyEmail - only set when nothing was
    // actually sent (no EmailSettings:Host configured) and we're outside
    // Production.
    public string? DevPreviewCode { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        if (string.IsNullOrWhiteSpace(Username))
            return RedirectToPage("/Account/ForgotPassword");

        MaskedEmail = Mask(Username);

        var officialId = await _authService.GetOfficialIdByUsernameAsync(Username);
        if (officialId is not null && !await _authService.HasPendingVerificationCodeAsync(officialId))
            await SendNewCodeAsync(officialId, Username);

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        MaskedEmail = Mask(Username);

        if (!ModelState.IsValid)
            return Page();

        var officialId = await _authService.GetOfficialIdByUsernameAsync(Username);
        var confirmed = officialId is not null && await _authService.ConfirmVerificationCodeAsync(officialId, Input.Code);

        if (!confirmed || officialId is null)
        {
            StatusIsError = true;
            StatusMessage = "That code is incorrect or has expired. Request a new one below.";
            return Page();
        }

        await _authService.ResetPasswordAsync(officialId, Input.NewPassword);

        TempData["StatusMessage"] = "Your password has been reset - sign in with your new password.";
        return RedirectToPage("/Account/Login");
    }

    public async Task<IActionResult> OnPostResendAsync()
    {
        MaskedEmail = Mask(Username);

        var officialId = await _authService.GetOfficialIdByUsernameAsync(Username);
        if (officialId is not null)
            await SendNewCodeAsync(officialId, Username);

        StatusMessage = "If that account exists, a new code has been sent.";
        return Page();
    }

    private async Task SendNewCodeAsync(string officialId, string email)
    {
        var code = await _authService.IssueVerificationCodeAsync(officialId);
        var actuallySent = await _emailService.SendVerificationCodeAsync(email, email, code);

        if (!actuallySent && !_env.IsProduction())
            DevPreviewCode = code;
    }

    private static string Mask(string? email)
    {
        if (string.IsNullOrEmpty(email) || !email.Contains('@'))
            return email ?? string.Empty;

        var parts = email.Split('@', 2);
        var visible = parts[0].Length <= 2 ? parts[0] : parts[0][..2];
        return $"{visible}***@{parts[1]}";
    }
}
