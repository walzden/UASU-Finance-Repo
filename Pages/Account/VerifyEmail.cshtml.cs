using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UASU_VoucherApprovals.Models;
using UASU_VoucherApprovals.Services;

namespace UASU_VoucherApprovals.Pages.Account;

// [Authorize] only, not [AllowAnonymous] - same reasoning as
// ChangePassword: the person must already be signed in (even with
// MustChangePassword=true/EmailVerified=false claims) to reach this
// page. The enforcement middleware in Program.cs is what routes them
// here right after login, before ChangePassword.
[Authorize]
public class VerifyEmailModel : PageModel
{
    private readonly IAuthService _authService;
    private readonly IEmailService _emailService;
    private readonly IWebHostEnvironment _env;

    public VerifyEmailModel(IAuthService authService, IEmailService emailService, IWebHostEnvironment env)
    {
        _authService = authService;
        _emailService = emailService;
        _env = env;
    }

    [BindProperty]
    public VerifyEmailInputModel Input { get; set; } = new();

    public string MaskedEmail { get; set; } = string.Empty;
    public string? StatusMessage { get; set; }
    public bool StatusIsError { get; set; }

    // Populated only when IEmailService reports it didn't actually send
    // (no EmailSettings:Host configured) AND we're outside Production -
    // keyed off the real send result, not just "is this Development",
    // so a real SMTP send never gets masked by a leftover preview banner
    // (see EmailService.SendVerificationCodeAsync's bool return).
    public string? DevPreviewCode { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var officialId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        if (await _authService.IsEmailVerifiedAsync(officialId))
            return RedirectToPage("/Account/ChangePassword");

        var email = await _authService.GetVerificationEmailAsync(officialId);
        MaskedEmail = Mask(email);

        // Don't burn a new code (and a new email) on every refresh -
        // only issue one if nothing valid is already pending.
        if (!await _authService.HasPendingVerificationCodeAsync(officialId))
            await SendNewCodeAsync(officialId, email);

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var officialId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        if (!ModelState.IsValid)
        {
            MaskedEmail = Mask(await _authService.GetVerificationEmailAsync(officialId));
            return Page();
        }

        var confirmed = await _authService.ConfirmVerificationCodeAsync(officialId, Input.Code);
        if (!confirmed)
        {
            StatusIsError = true;
            StatusMessage = "That code is incorrect or has expired. Request a new one below.";
            MaskedEmail = Mask(await _authService.GetVerificationEmailAsync(officialId));
            return Page();
        }

        // Re-issue the cookie with EmailVerified set, same pattern as
        // ChangePassword clearing MustChangePassword - the enforcement
        // middleware reads this claim on every subsequent request.
        var identity = (ClaimsIdentity)User.Identity!;
        var existing = identity.FindFirst("EmailVerified");
        if (existing is not null)
            identity.RemoveClaim(existing);
        identity.AddClaim(new Claim("EmailVerified", "true"));

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

        return RedirectToPage("/Account/ChangePassword");
    }

    public async Task<IActionResult> OnPostResendAsync()
    {
        var officialId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var email = await _authService.GetVerificationEmailAsync(officialId);

        await SendNewCodeAsync(officialId, email);

        MaskedEmail = Mask(email);
        StatusMessage = "A new code has been sent.";
        return Page();
    }

    private async Task SendNewCodeAsync(string officialId, string? email)
    {
        var code = await _authService.IssueVerificationCodeAsync(officialId);
        if (string.IsNullOrEmpty(email))
            return;

        var actuallySent = await _emailService.SendVerificationCodeAsync(email, User.Identity!.Name!, code);

        // Only show the code on-screen when nothing was actually sent
        // (no EmailSettings:Host configured) - a real send must never be
        // masked by a leftover preview banner, in Development or not.
        if (!actuallySent && !_env.IsProduction())
            DevPreviewCode = code;
    }

    private static string Mask(string? email)
    {
        if (string.IsNullOrEmpty(email) || !email.Contains('@'))
            return email ?? "(no email on file)";

        var parts = email.Split('@', 2);
        var visible = parts[0].Length <= 2 ? parts[0] : parts[0][..2];
        return $"{visible}***@{parts[1]}";
    }
}
