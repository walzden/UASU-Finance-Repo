using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UASU_VoucherApprovals.Models;
using UASU_VoucherApprovals.Services;
using UASU_VoucherApprovals.Utilities;

namespace UASU_VoucherApprovals.Pages.Payments;

[Authorize(Policy = "TreasuryAdmin")]
[RequestSizeLimit(6_000_000)]
public class AcknowledgeModel : PageModel
{
    private readonly IVoucherService _voucherService;

    public AcknowledgeModel(IVoucherService voucherService)
    {
        _voucherService = voucherService;
    }

    [BindProperty]
    public AcknowledgementInputModel Input { get; set; } = new();

    public IEnumerable<PaymentAcknowledgementOption> AwaitingPayments { get; set; } = Enumerable.Empty<PaymentAcknowledgementOption>();
    public IEnumerable<AcknowledgementRow> RecentAcknowledgements { get; set; } = Enumerable.Empty<AcknowledgementRow>();

    // [TempData] rather than a plain property - it needs to survive the
    // redirect below (a redirect starts a brand-new request/PageModel,
    // so a normal property would just be lost).
    [TempData]
    public string? StatusMessage { get; set; }

    [TempData]
    public bool StatusIsError { get; set; }

    // ---- Batch: everything waiting, grouped by withdrawal ----

    public List<AcknowledgementBacklogRow> Backlog { get; set; } = new();

    [BindProperty]
    public List<AcknowledgementLineInput> Lines { get; set; } = new();

    // Shown when a batch is refused; the ticks and edits are kept
    // (Posted) so nothing has to be re-entered.
    public string? BatchError { get; set; }
    public Dictionary<string, AcknowledgementLineInput> Posted { get; private set; } = new();

    public async Task OnGetAsync()
    {
        AwaitingPayments = await _voucherService.GetPaymentsAwaitingAcknowledgementAsync();
        RecentAcknowledgements = await _voucherService.GetRecentAcknowledgementsAsync();
        Backlog = (await _voucherService.GetAcknowledgementBacklogAsync()).ToList();
    }

    public async Task<IActionResult> OnPostBatchAsync()
    {
        Backlog = (await _voucherService.GetAcknowledgementBacklogAsync()).ToList();

        // Only payments still waiting can be acknowledged from here.
        var waiting = Backlog.Select(b => b.Payment_ID).ToHashSet();
        var ticked = Lines.Where(l => l.Selected && waiting.Contains(l.Payment_ID)).ToList();

        try
        {
            if (ticked.Count == 0)
                throw new InvalidOperationException("Tick at least one payment to acknowledge.");

            var n = await _voucherService.RecordAcknowledgementsAsync(ticked);
            StatusMessage = $"{n} acknowledgement{(n == 1 ? "" : "s")} logged.";
            return LocalRedirect(Url.Content("~/Payments/Acknowledge"));
        }
        catch (InvalidOperationException ex)
        {
            BatchError = ex.Message;
            Posted = Lines.Where(l => !string.IsNullOrEmpty(l.Payment_ID))
                          .GroupBy(l => l.Payment_ID).ToDictionary(g => g.Key, g => g.First());
            AwaitingPayments = await _voucherService.GetPaymentsAwaitingAcknowledgementAsync();
            RecentAcknowledgements = await _voucherService.GetRecentAcknowledgementsAsync();
            return Page();
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        AwaitingPayments = await _voucherService.GetPaymentsAwaitingAcknowledgementAsync();
        Backlog = (await _voucherService.GetAcknowledgementBacklogAsync()).ToList();

        if (!ModelState.IsValid)
        {
            RecentAcknowledgements = await _voucherService.GetRecentAcknowledgementsAsync();
            return Page();
        }

        try
        {
            await _voucherService.RecordAcknowledgementAsync(Input);
            StatusMessage = $"Acknowledgement logged for payment {Input.Payment_ID}.";

            // Redirect instead of returning Page() directly, so a
            // refresh after a successful submit issues a fresh GET
            // rather than resubmitting the same acknowledgement (the
            // browser's "confirm form resubmission" prompt).
            return RedirectToPage();
        }
        catch (InvalidOperationException ex)
        {
            // Attachment failed the size/type check in the service. Stay
            // on the POST response (no redirect) so what they already
            // typed - payee, notes, etc. - isn't lost just because the
            // attachment needs fixing.
            StatusIsError = true;
            StatusMessage = ex.Message;
        }

        RecentAcknowledgements = await _voucherService.GetRecentAcknowledgementsAsync();
        return Page();
    }

    // Full history, not just the 5 shown on screen - metadata only
    // (whether an attachment exists), not the image itself.
    public async Task<IActionResult> OnGetExportCsvAsync()
    {
        var rows = await _voucherService.GetAllAcknowledgementsAsync();
        var csv = CsvExport.Build(
            new[] { "Payment", "Date", "Acknowledged By", "Method", "Notes", "Has Attachment" },
            rows.Select(a => new object?[] { a.Payment_ID, a.Acknowledgement_Date, a.Acknowledged_By, a.Method, a.Notes, a.HasAttachment }));

        return File(csv, "text/csv", "PaymentAcknowledgements.csv");
    }
}
