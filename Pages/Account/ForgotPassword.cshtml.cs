using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UASU_VoucherApprovals.Models;

namespace UASU_VoucherApprovals.Pages.Account;

// No [Authorize] - this is reached before signing in. Doesn't look up
// the account at all; ResetPassword's own OnGetAsync does that (and
// issues/sends the code), same split VerifyEmail uses between the page
// that collects input and the page that actually sends the code.
public class ForgotPasswordModel : PageModel
{
    [BindProperty]
    public ForgotPasswordInputModel Input { get; set; } = new();

    public void OnGet() { }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
            return Page();

        return RedirectToPage("/Account/ResetPassword", new { username = Input.Username });
    }
}
