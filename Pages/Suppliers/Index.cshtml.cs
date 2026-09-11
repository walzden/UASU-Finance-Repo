using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UASU_VoucherApprovals.Models;
using UASU_VoucherApprovals.Services;

namespace UASU_VoucherApprovals.Pages.Suppliers;

[Authorize(Policy = "TreasuryAdmin")]
public class IndexModel : PageModel
{
    private readonly IReferenceDataService _referenceDataService;

    public IndexModel(IReferenceDataService referenceDataService)
    {
        _referenceDataService = referenceDataService;
    }

    [BindProperty]
    public SupplierInputModel Input { get; set; } = new();

    public IEnumerable<SupplierRow> Suppliers { get; set; } = Enumerable.Empty<SupplierRow>();

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task OnGetAsync()
    {
        Suppliers = await _referenceDataService.GetSuppliersAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            Suppliers = await _referenceDataService.GetSuppliersAsync();
            return Page();
        }

        var supplierId = await _referenceDataService.CreateSupplierAsync(Input);
        StatusMessage = $"Added {Input.Business_Name} ({supplierId}).";

        return RedirectToPage();
    }
}
