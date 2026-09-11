using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UASU_VoucherApprovals.Models;
using UASU_VoucherApprovals.Services;

namespace UASU_VoucherApprovals.Pages.Suppliers;

[Authorize(Policy = "TreasuryAdmin")]
public class EditModel : PageModel
{
    private readonly IReferenceDataService _referenceDataService;

    public EditModel(IReferenceDataService referenceDataService)
    {
        _referenceDataService = referenceDataService;
    }

    [BindProperty(SupportsGet = true)]
    public string SupplierId { get; set; } = string.Empty;

    [BindProperty]
    public SupplierInputModel Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var existing = await _referenceDataService.GetSupplierForEditAsync(SupplierId);
        if (existing is null)
            return NotFound();

        Input = existing;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        await _referenceDataService.UpdateSupplierAsync(SupplierId, Input);

        // The key here matches IndexModel's [TempData] StatusMessage
        // property by name, so it shows up there after the redirect.
        TempData["StatusMessage"] = $"Updated {Input.Business_Name} ({SupplierId}).";
        return RedirectToPage("/Suppliers/Index");
    }
}
