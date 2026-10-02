using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using UASU_VoucherApprovals.Models;
using UASU_VoucherApprovals.Services;

namespace UASU_VoucherApprovals.Pages.Floats;

// One float: record receipts, retire them onto vouchers for approval,
// settle approved vouchers, record returned cash, close the float, and
// print its statement. The triggers in SQL/032 re-check every rule here;
// their messages come back as SqlException and are shown as-is.
[Authorize(Policy = "TreasuryAdmin")]
public class DetailsModel : PageModel
{
    private readonly ICashFloatService _floatService;

    public DetailsModel(ICashFloatService floatService)
    {
        _floatService = floatService;
    }

    [BindProperty(SupportsGet = true)]
    public string Id { get; set; } = string.Empty;

    [BindProperty]
    public CashFloatReceiptInputModel ReceiptInput { get; set; } = new();

    [BindProperty]
    public CashFloatReturnInputModel ReturnInput { get; set; } = new();

    public CashFloatRow? Float { get; set; }
    public IEnumerable<CashFloatReceiptRow> Receipts { get; set; } = Enumerable.Empty<CashFloatReceiptRow>();
    public IEnumerable<CashFloatVoucherRow> Vouchers { get; set; } = Enumerable.Empty<CashFloatVoucherRow>();
    public IEnumerable<CashFloatReturnRow> Returns { get; set; } = Enumerable.Empty<CashFloatReturnRow>();
    public IEnumerable<SimpleOption> BudgetCodes { get; set; } = Enumerable.Empty<SimpleOption>();

    // See Floats/Index for why success and error are kept apart.
    [TempData]
    public string? StatusMessage { get; set; }
    public string? ErrorMessage { get; set; }

    private string CurrentOfficialId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    public async Task<IActionResult> OnGetAsync()
    {
        return await LoadAsync() ? Page() : NotFound();
    }

    public async Task<IActionResult> OnPostAddReceiptAsync()
    {
        ReceiptInput.Float_ID = Id;
        return await RunAsync(nameof(ReceiptInput), ReceiptInput, async () =>
        {
            await _floatService.AddReceiptAsync(ReceiptInput, CurrentOfficialId);
            return $"Receipt from {ReceiptInput.Vendor} for Sh.{ReceiptInput.Amount:N2} recorded.";
        });
    }

    public async Task<IActionResult> OnPostDeleteReceiptAsync(int receiptLineId)
    {
        return await RunAsync(null, null, async () =>
        {
            await _floatService.DeleteReceiptAsync(Id, receiptLineId);
            return "Receipt removed.";
        });
    }

    public async Task<IActionResult> OnPostRetireAsync(List<int> receiptIds)
    {
        return await RunAsync(null, null, async () =>
        {
            var voucherIds = await _floatService.RetireAsync(Id, receiptIds);
            return $"Raised {string.Join(", ", voucherIds)} for approval. Settle {(voucherIds.Count == 1 ? "it" : "them")} here once the Chairman and Chapter Secretary have approved.";
        });
    }

    public async Task<IActionResult> OnPostSettleAsync(List<string> voucherIds)
    {
        return await RunAsync(null, null, async () =>
        {
            var paymentId = await _floatService.SettleAsync(Id, voucherIds);
            return $"Settled as cash payment {paymentId}. The receipts are now in the cash book.";
        });
    }

    public async Task<IActionResult> OnPostReturnAsync()
    {
        ReturnInput.Float_ID = Id;
        return await RunAsync(nameof(ReturnInput), ReturnInput, async () =>
        {
            await _floatService.RecordReturnAsync(ReturnInput, CurrentOfficialId);
            return $"Sh.{ReturnInput.Amount:N2} returned to the treasury recorded.";
        });
    }

    public async Task<IActionResult> OnPostCloseAsync()
    {
        return await RunAsync(null, null, async () =>
        {
            await _floatService.CloseAsync(Id);
            return $"Float {Id} closed.";
        });
    }

    // Several forms share this page: validate only the one submitted (the
    // others' untouched fields would otherwise fail their own rules), run
    // the action, then redirect on success or re-render with the error.
    private async Task<IActionResult> RunAsync(string? prefix, object? input, Func<Task<string>> action)
    {
        ModelState.Clear();
        if (input is not null && !TryValidateModel(input, prefix!))
            return await LoadAsync() ? Page() : NotFound();

        try
        {
            StatusMessage = await action();
            return RedirectToPage(new { id = Id });
        }
        catch (Exception ex) when (ex is SqlException or InvalidOperationException)
        {
            ErrorMessage = ex.Message;
        }

        return await LoadAsync() ? Page() : NotFound();
    }

    private async Task<bool> LoadAsync()
    {
        Float = await _floatService.GetFloatAsync(Id);
        if (Float is null)
            return false;

        Receipts = await _floatService.GetReceiptsAsync(Id);
        Vouchers = await _floatService.GetRetirementVouchersAsync(Id);
        Returns = await _floatService.GetReturnsAsync(Id);
        BudgetCodes = await _floatService.GetExpenseBudgetCodesAsync();
        return true;
    }
}
