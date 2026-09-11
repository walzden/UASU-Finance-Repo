using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UASU_VoucherApprovals.Models;
using UASU_VoucherApprovals.Services;
using UASU_VoucherApprovals.Utilities;

namespace UASU_VoucherApprovals.Pages.Reports;

// Open to any signed-in user, same as the other reports - it's
// read-only browsing, nothing here creates or changes anything.
[Authorize]
public class ApprovedVouchersModel : PageModel
{
    private readonly IVoucherService _voucherService;

    public ApprovedVouchersModel(IVoucherService voucherService)
    {
        _voucherService = voucherService;
    }

    [BindProperty(SupportsGet = true, Name = "period")]
    public ReportPeriod SelectedPeriod { get; set; } = ReportPeriod.Monthly;

    [BindProperty(SupportsGet = true, Name = "year")]
    public int SelectedYear { get; set; }

    // Month (1-12), Quarter (1-4), or Half (1-2) depending on
    // SelectedPeriod - unused for Yearly, which covers the whole year.
    [BindProperty(SupportsGet = true, Name = "sub")]
    public int? SelectedSub { get; set; }

    public List<ApprovedVoucherRow> Vouchers { get; set; } = new();
    public ILookup<string, VoucherApprovalActionRow> ApprovalsByVoucher { get; set; } = Enumerable.Empty<VoucherApprovalActionRow>().ToLookup(a => a.Voucher_ID);
    public ILookup<string, VoucherPaymentRow> PaymentsByVoucher { get; set; } = Enumerable.Empty<VoucherPaymentRow>().ToLookup(p => p.Voucher_ID);

    public DateTime PeriodStart { get; private set; }
    public DateTime PeriodEnd { get; private set; }
    public string PeriodLabel { get; private set; } = string.Empty;

    public async Task OnGetAsync()
    {
        if (SelectedYear == 0)
            SelectedYear = DateTime.Today.Year;

        if (SelectedSub is null)
        {
            SelectedSub = SelectedPeriod switch
            {
                ReportPeriod.Monthly => DateTime.Today.Month,
                ReportPeriod.Quarterly => (DateTime.Today.Month - 1) / 3 + 1,
                ReportPeriod.HalfYearly => DateTime.Today.Month <= 6 ? 1 : 2,
                _ => null
            };
        }

        (PeriodStart, PeriodEnd, PeriodLabel) = ComputeRange(SelectedPeriod, SelectedYear, SelectedSub);

        Vouchers = (await _voucherService.GetApprovedVouchersAsync(PeriodStart, PeriodEnd)).ToList();
        ApprovalsByVoucher = (await _voucherService.GetVoucherApprovalActionsAsync(PeriodStart, PeriodEnd)).ToLookup(a => a.Voucher_ID);
        PaymentsByVoucher = (await _voucherService.GetVoucherPaymentsAsync(PeriodStart, PeriodEnd)).ToLookup(p => p.Voucher_ID);
    }

    // Turns (period type, year, sub-period) into a [start, end) date
    // range and a human label - a plain range filter rather than the
    // period-bucketed views the other reports use (IncomeExpenseByMonth
    // etc.), since this page narrows down to ONE period's worth of
    // vouchers rather than aggregating totals across every period.
    private static (DateTime Start, DateTime End, string Label) ComputeRange(ReportPeriod period, int year, int? sub)
    {
        switch (period)
        {
            case ReportPeriod.Monthly:
                var month = sub ?? 1;
                var monthStart = new DateTime(year, month, 1);
                return (monthStart, monthStart.AddMonths(1), monthStart.ToString("MMMM yyyy"));

            case ReportPeriod.Quarterly:
                var quarter = sub ?? 1;
                var qStart = new DateTime(year, (quarter - 1) * 3 + 1, 1);
                return (qStart, qStart.AddMonths(3), $"Q{quarter} {year}");

            case ReportPeriod.HalfYearly:
                var half = sub ?? 1;
                var hStart = new DateTime(year, half == 1 ? 1 : 7, 1);
                return (hStart, hStart.AddMonths(6), $"H{half} {year}");

            default: // Yearly
                var yStart = new DateTime(year, 1, 1);
                return (yStart, yStart.AddYears(1), year.ToString());
        }
    }

    public async Task<IActionResult> OnGetExportCsvAsync()
    {
        if (SelectedYear == 0)
            SelectedYear = DateTime.Today.Year;

        (PeriodStart, PeriodEnd, PeriodLabel) = ComputeRange(SelectedPeriod, SelectedYear, SelectedSub);

        var vouchers = (await _voucherService.GetApprovedVouchersAsync(PeriodStart, PeriodEnd)).ToList();
        var approvals = (await _voucherService.GetVoucherApprovalActionsAsync(PeriodStart, PeriodEnd)).ToLookup(a => a.Voucher_ID);
        var payments = (await _voucherService.GetVoucherPaymentsAsync(PeriodStart, PeriodEnd)).ToLookup(p => p.Voucher_ID);

        var csv = CsvExport.Build(
            new[]
            {
                "Voucher", "Date", "Type", "Payee", "Amount", "Description", "Budget Category",
                "Status", "Chairman", "Chairman Date", "Chapter Secretary", "Chapter Secretary Date",
                "Payment", "Payment Date", "Payment Mode"
            },
            vouchers.Select(v =>
            {
                var chairman = approvals[v.Voucher_ID].FirstOrDefault(a => a.ApproverRole == "Chairman");
                var chapterSec = approvals[v.Voucher_ID].FirstOrDefault(a => a.ApproverRole == "Chapter Secretary");
                var payment = payments[v.Voucher_ID].FirstOrDefault();

                return new object?[]
                {
                    v.Voucher_ID, v.VoucherDate, v.Transaction_Type, v.PayeeDisplay, v.Amount, v.Description, v.Budget_Category,
                    v.Is_Legacy ? "Legacy" : v.Current_Status,
                    chairman?.Approval_Status, chairman?.Approval_Date,
                    chapterSec?.Approval_Status, chapterSec?.Approval_Date,
                    payment?.Payment_ID, payment?.Payment_Date, payment?.Payment_Mode
                };
            }));

        return File(csv, "text/csv", $"ApprovedVouchers_{PeriodLabel.Replace(" ", "")}.csv");
    }
}
