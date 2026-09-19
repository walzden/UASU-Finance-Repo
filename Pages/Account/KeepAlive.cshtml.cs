using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UASU_VoucherApprovals.Pages.Account;

// Pinged by wwwroot/js/idle-timeout.js when the user clicks "Stay signed
// in" - the cookie's sliding expiration already renews on any authenticated
// request, so this just needs to require auth and return instantly.
[Authorize]
public class KeepAliveModel : PageModel
{
    public IActionResult OnGet() => new EmptyResult();
}
