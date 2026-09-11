using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UASU_VoucherApprovals.Models;
using UASU_VoucherApprovals.Services;
using UASU_VoucherApprovals.Utilities;

namespace UASU_VoucherApprovals.Pages.Reports;

// [Authorize] only, read-only - same as Form R. One Form Q (Treasurer's
// Receipt) per income payment received from the National Office, either
// as member subscriptions or a grant - mirrors the physical UASU Form Q
// receipt book. Income vouchers skip the Chairman/Chapter Secretary
// approval workflow entirely (auto-approved on entry, per CLAUDE.md), so
// unlike Form R there's no approval stamp here - just the fixed
// four-row "Being payment of" breakdown the physical form itself has.
[Authorize]
public class FormQModel : PageModel
{
    private readonly IVoucherService _voucherService;

    public FormQModel(IVoucherService voucherService)
    {
        _voucherService = voucherService;
    }

    [BindProperty(SupportsGet = true, Name = "mode")]
    public string Mode { get; set; } = "single";

    [BindProperty(SupportsGet = true, Name = "year")]
    public int SelectedYear { get; set; }

    [BindProperty(SupportsGet = true, Name = "payment")]
    public string? SelectedPaymentId { get; set; }

    public List<ReceiptOption> Receipts { get; set; } = new();
    public List<int> AvailableYears { get; set; } = new();
    public List<FormQData> FormQs { get; set; } = new();

    public record ReceiptOption(string Payment_ID, DateTime Payment_Date, decimal Total);

    // Fixed order matching the physical form's pre-printed rows -
    // always all four, even at zero, rather than only showing categories
    // that have an amount.
    private static readonly string[] Buckets = { "Monthly Subscription", "Entrance Fees", "Donations", "Others" };

    public record FormQData(
        string Payment_ID, DateTime Payment_Date, string Payment_Mode, string? Reference_No,
        string PayerName, Dictionary<string, decimal> AmountsByBucket, decimal Total, string AmountInWords);

    public async Task OnGetAsync()
    {
        var allRows = (await _voucherService.GetIncomeReceiptsAsync())
            // Form Q is specifically for receipts from the National
            // Office (subscriptions/grants) - not every Income voucher
            // necessarily is, even though every one currently is.
            .Where(r => !string.IsNullOrWhiteSpace(r.PayerName) && r.PayerName.Contains("National", StringComparison.OrdinalIgnoreCase))
            .ToList();

        var byPayment = allRows.GroupBy(r => r.Payment_ID).ToList();

        Receipts = byPayment
            .Select(g => new ReceiptOption(g.Key, g.First().Payment_Date, g.Sum(r => r.AllocatedFromThisPayment)))
            .OrderByDescending(r => r.Payment_Date)
            .ToList();

        AvailableYears = Receipts.Select(r => r.Payment_Date.Year).Distinct().OrderByDescending(y => y).ToList();
        if (SelectedYear == 0)
            SelectedYear = AvailableYears.FirstOrDefault(y => y == DateTime.Today.Year, AvailableYears.FirstOrDefault());

        List<IGrouping<string, IncomeReceiptVoucherRow>> targets;
        if (Mode == "all")
        {
            targets = byPayment.Where(g => g.First().Payment_Date.Year == SelectedYear)
                .OrderBy(g => g.First().Payment_Date)
                .ToList();
        }
        else
        {
            if (string.IsNullOrEmpty(SelectedPaymentId))
            {
                SelectedPaymentId = Receipts.FirstOrDefault(r => r.Payment_Date.Year == SelectedYear)?.Payment_ID
                    ?? Receipts.FirstOrDefault()?.Payment_ID;
            }

            var single = byPayment.FirstOrDefault(g => g.Key == SelectedPaymentId);
            targets = single is not null ? new List<IGrouping<string, IncomeReceiptVoucherRow>> { single } : new List<IGrouping<string, IncomeReceiptVoucherRow>>();
        }

        FormQs = targets.Select(BuildFormQData).ToList();
    }

    private static FormQData BuildFormQData(IGrouping<string, IncomeReceiptVoucherRow> group)
    {
        var first = group.First();
        var amountsByBucket = Buckets.ToDictionary(b => b, b => 0m);

        foreach (var row in group)
            amountsByBucket[BucketFor(row.Category_Name)] += row.AllocatedFromThisPayment;

        var total = amountsByBucket.Values.Sum();

        return new FormQData(
            first.Payment_ID, first.Payment_Date, first.Payment_Mode, first.Reference_No,
            first.PayerName ?? "UASU National Office", amountsByBucket, total, NumberToWords.ConvertShillings(total));
    }

    private static string BucketFor(string? categoryName)
    {
        var name = categoryName ?? "";
        if (name.Contains("Subscription", StringComparison.OrdinalIgnoreCase)) return "Monthly Subscription";
        if (name.Contains("Entrance", StringComparison.OrdinalIgnoreCase)) return "Entrance Fees";
        if (name.Contains("Donation", StringComparison.OrdinalIgnoreCase)) return "Donations";
        return "Others";
    }
}
