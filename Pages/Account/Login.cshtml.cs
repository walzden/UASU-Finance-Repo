using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UASU_VoucherApprovals.Services;

namespace UASU_VoucherApprovals.Pages.Account;

public class LoginModel : PageModel
{
    private readonly IAuthService _authService;

    public LoginModel(IAuthService authService)
    {
        _authService = authService;
    }

    [BindProperty]
    [Required(ErrorMessage = "Please enter your username.")]
    public string Username { get; set; } = string.Empty;

    [BindProperty]
    [Required(ErrorMessage = "Please enter your password.")]
    public string Password { get; set; } = string.Empty;

    public string? ErrorMessage { get; set; }

    // Set by ResetPasswordModel via TempData after a successful reset.
    [TempData]
    public string? StatusMessage { get; set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        // A blank username/password never reaches ValidateCredentialsAsync -
        // BCrypt.Verify throws on some malformed inputs, which without this
        // guard surfaced as the generic "Something went wrong" error page
        // in production instead of a normal validation message.
        if (!ModelState.IsValid)
        {
            ErrorMessage = "Please enter both your username and password.";
            return Page();
        }

        var user = await _authService.ValidateCredentialsAsync(Username, Password);

        if (user is null)
        {
            ErrorMessage = "Invalid username or password.";
            return Page();
        }

        // Claims drive both the navbar and the [Authorize(Roles=...)]
        // checks on the approvals page - Role comes straight from
        // Ref_Officials.Role at login time. MustChangePassword/
        // EmailVerified are claims (not just a redirect here) so the
        // enforcement middleware in Program.cs can catch every
        // subsequent request, not just this one.
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.OfficialID),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Role, user.Role),
            new("MustChangePassword", user.MustChangePassword ? "true" : "false"),
            new("EmailVerified", user.EmailVerified ? "true" : "false")
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

        await _authService.RecordLoginAsync(user.OfficialID);

        // Email verification (confirming Username is a real, reachable
        // address) comes before the forced password change - no point
        // letting someone set a real password on an account seeded with
        // a dummy address.
        if (user.MustChangePassword && !user.EmailVerified)
            return RedirectToPage("/Account/VerifyEmail");

        if (user.MustChangePassword)
            return RedirectToPage("/Account/ChangePassword");

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return LocalRedirect(returnUrl);

        return RedirectToPage("/Index");
    }
}
