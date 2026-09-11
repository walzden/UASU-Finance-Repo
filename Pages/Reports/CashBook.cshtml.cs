using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UASU_VoucherApprovals.Models;
using UASU_VoucherApprovals.Services;
using UASU_VoucherApprovals.Utilities;

namespace UASU_VoucherApprovals.Pages.Reports;

// [Authorize] only, no role restriction - same as every other report,
// open to anyone with a login.
[Authorize]
public class CashBookModel : PageModel
{
    private readonly IReportService _reportService;

    public CashBookModel(IReportService reportService)
    {
        _reportService = reportService;
    }

    [BindProperty(SupportsGet = true, Name = "year")]
    public int SelectedYear { get; set; }

    public IEnumerable<CashBookRow> Rows { get; set; } = Enumerable.Empty<CashBookRow>();
    public IEnumerable<int> AvailableYears { get; set; } = Enumerable.Empty<int>();

    public async Task OnGetAsync()
    {
        AvailableYears = await _reportService.GetAvailableYearsAsync();

        if (!Request.Query.ContainsKey("year"))
            SelectedYear = DateTime.Today.Year;

        Rows = (await _reportService.GetCashBookAsync(SelectedYear)).ToList();
    }

    public async Task<IActionResult> OnGetExportCsvAsync()
    {
        var rows = await _reportService.GetCashBookAsync(SelectedYear);
        var csv = CsvExport.Build(
            new[] { "Date", "Item", "Voucher/Receipt No.", "Received Cash", "Received Bank", "Paid Cash", "Paid Bank", "Cash Balance", "Bank Balance" },
            rows.Select(r => new object?[]
            {
                r.EntryDate, r.Item, r.VoucherOrReceiptNo,
                r.ReceivedCash, r.ReceivedBank, r.PaidCash, r.PaidBank,
                r.RunningCashBalance, r.RunningBankBalance
            }));

        return File(csv, "text/csv", $"CashBook_{SelectedYear}.csv");
    }

    // The full Voucher_ID ("UASU-MMU-0106/26") is wider than the printed
    // column can hold without overrunning the next one - even dropping
    // just the "UASU-" prefix ("MMU-0106/26") still overran, so this
    // keeps only the counter/year after the LAST '-' ("0106/26"), which
    // is what actually distinguishes one entry from another day-to-day.
    // CSV export above keeps the full ID; this is display-only.
    // Aggregated rows ("3 vouchers") have no '-' and pass through as-is.
    // Withdrawal_IDs are shorter to begin with and are shown in full -
    // see below.
    public static string ShortVoucherNo(string? voucherOrReceiptNo)
    {
        if (string.IsNullOrEmpty(voucherOrReceiptNo)) return voucherOrReceiptNo ?? "";
        var lastDash = voucherOrReceiptNo.LastIndexOf('-');
        return lastDash >= 0 ? voucherOrReceiptNo[(lastDash + 1)..] : voucherOrReceiptNo;
    }
}
