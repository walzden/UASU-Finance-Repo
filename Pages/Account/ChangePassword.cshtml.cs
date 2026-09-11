using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UASU_VoucherApprovals.Models;
using UASU_VoucherApprovals.Services;

namespace UASU_VoucherApprovals.Pages.Account;

// [Authorize] only, not [AllowAnonymous] - the person must already be
// signed in (even with a MustChangePassword=true cookie) to reach this
// page. The enforcement middleware in Program.cs is what routes them
// here in the first place.
[Authorize]
public class ChangePasswordModel : PageModel
{
    private readonly IAuthService _authService;

    public ChangePasswordModel(IAuthService authService)
    {
        _authService = authService;
    }

    [BindProperty]
    public ChangePasswordInputModel Input { get; set; } = new();

    public bool IsForced => User.FindFirstValue("MustChangePassword") == "true";
    public string? StatusMessage { get; set; }
    public bool StatusIsError { get; set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        var officialId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var success = await _authService.ChangePasswordAsync(officialId, Input.CurrentPassword, Input.NewPassword);

        if (!success)
        {
            StatusIsError = true;
            StatusMessage = "Current password is incorrect.";
            return Page();
        }

        // Re-issue the cookie with MustChangePassword cleared, so the
        // enforcement middleware stops redirecting here on every request.
        var identity = (ClaimsIdentity)User.Identity!;
        identity.RemoveClaim(identity.FindFirst("MustChangePassword"));
        identity.AddClaim(new Claim("MustChangePassword", "false"));

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

        return RedirectToPage("/Index");
    }
}
