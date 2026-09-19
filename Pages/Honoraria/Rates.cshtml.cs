using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UASU_VoucherApprovals.Models;
using UASU_VoucherApprovals.Services;

namespace UASU_VoucherApprovals.Pages.Honoraria;

[Authorize(Policy = "TreasuryAdmin")]
public class RatesModel : PageModel
{
    private readonly IHonorariaService _honorariaService;

    public RatesModel(IHonorariaService honorariaService)
    {
        _honorariaService = honorariaService;
    }

    [BindProperty]
    public List<HonorariaRateInputModel> Rates { get; set; } = new();

    [BindProperty]
    public HonorariaRateInputModel NewRate { get; set; } = new();

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task OnGetAsync()
    {
        await LoadRatesAsync();
    }

    // The page has two forms bound to two different properties; posting one
    // still validates the other's (empty) model, so each handler discards
    // the validation entries that belong to the form it isn't handling.
    // Matched by the "Rates[" key prefix rather than by NewRate's key names,
    // since the latter aren't reliable for an unbound property.
    // ratesForm = true keeps only the Rates[...] entries; false keeps
    // everything except them.
    private void DiscardModelState(bool ratesForm)
    {
        foreach (var key in ModelState.Keys
                     .Where(k => k.StartsWith("Rates[") != ratesForm)
                     .ToList())
            ModelState.Remove(key);
    }

    // Plain redirect to the page URL: RedirectToPage() from a named handler
    // would carry "?handler=Save" onto the reloaded page.
    private IActionResult BackToRates() => LocalRedirect("/Honoraria/Rates");

    public async Task<IActionResult> OnPostSaveAsync()
    {
        DiscardModelState(ratesForm: true);

        if (!ModelState.IsValid)
            return Page();

        foreach (var rate in Rates)
            await _honorariaService.SetRateAsync(rate.Role, rate.Amount);

        StatusMessage = "Rates saved.";
        return BackToRates();
    }

    public async Task<IActionResult> OnPostAddAsync()
    {
        DiscardModelState(ratesForm: false);

        if (!ModelState.IsValid)
        {
            await LoadRatesAsync();
            return Page();
        }

        await _honorariaService.SetRateAsync(NewRate.Role.Trim(), NewRate.Amount);
        StatusMessage = $"{NewRate.Role.Trim()} set to {NewRate.Amount:N2}.";
        return BackToRates();
    }

    public async Task<IActionResult> OnPostDeleteAsync(string role)
    {
        await _honorariaService.DeleteRateAsync(role);
        StatusMessage = $"Removed {role}.";
        return BackToRates();
    }

    private async Task LoadRatesAsync()
    {
        Rates = (await _honorariaService.GetRatesAsync())
            .Select(r => new HonorariaRateInputModel { Role = r.Role, Amount = r.Amount })
            .ToList();
    }
}
