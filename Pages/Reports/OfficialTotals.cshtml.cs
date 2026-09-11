using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UASU_VoucherApprovals.Models;
using UASU_VoucherApprovals.Services;

namespace UASU_VoucherApprovals.Pages.Reports;

[Authorize] // open to any signed-in user, same as the other reports
public class OfficialTotalsModel : PageModel
{
    private readonly IReportService _reportService;

    public OfficialTotalsModel(IReportService reportService)
    {
        _reportService = reportService;
    }

    // Null represents the "Total (ITD)" tab, which has no period
    // dimension at all - not worth adding a fifth ReportPeriod enum
    // value just to represent "no period."
    [BindProperty(SupportsGet = true, Name = "period")]
    public ReportPeriod? SelectedPeriod { get; set; }

    [BindProperty(SupportsGet = true, Name = "year")]
    public int? SelectedYear { get; set; }

    public bool IsItd => SelectedPeriod is null;

    // Yearly shows every year as its own column, so - unlike Monthly/
    // Quarterly/HalfYearly, which need one year picked to keep the
    // column count sane - it has no picker at all.
    public bool ShowYearPicker => SelectedPeriod is not null && SelectedPeriod != ReportPeriod.Yearly;

    public IEnumerable<int> AvailableYears { get; set; } = Enumerable.Empty<int>();

    // Pivot data - officials as rows (ranked by Total, richest first),
    // periods as columns. A period/official combination with no
    // payment simply has no key in ByPeriod, which the view renders as
    // an em dash rather than a misleading 0.00.
    public List<string> PeriodColumns { get; set; } = new();
    public List<OfficialPivotRow> Rows { get; set; } = new();
    public Dictionary<string, decimal> ColumnTotals { get; set; } = new();
    public decimal GrandTotal { get; set; }

    // ITD data
    public List<OfficialLifetimeTotal> LifetimeRows { get; set; } = new();

    // Budget-line breakdown behind each official's overall Total, keyed
    // by OfficialID - used for the ITD table's Total Paid cell and the
    // pivot table's Total column. Same year filter as whichever totals
    // query produced that Total (null for ITD and the Yearly tab), so
    // the lines here always sum back to it exactly.
    public Dictionary<string, List<OfficialBudgetLineTotal>> BudgetBreakdownTotal { get; set; } = new();

    // Budget-line breakdown behind one specific period cell, keyed by
    // (OfficialID, PeriodLabel) - e.g. just June 2026 for one official,
    // not their whole row. Only populated for the pivot table (Monthly/
    // Quarterly/HalfYearly/Yearly); the ITD table has no period columns.
    public Dictionary<(string OfficialID, string PeriodLabel), List<OfficialBudgetLineTotal>> BudgetBreakdownByPeriod { get; set; } = new();

    public record OfficialPivotRow(string OfficialID, string FullName, string? Role, Dictionary<string, decimal> ByPeriod, decimal Total);

    public async Task OnGetAsync()
    {
        AvailableYears = await _reportService.GetAvailableYearsAsync();

        if (IsItd)
        {
            LifetimeRows = (await _reportService.GetOfficialLifetimeTotalsAsync()).ToList();
            GrandTotal = LifetimeRows.Sum(r => r.TotalPaid);
            await LoadBudgetBreakdownAsync(null, null);
            return;
        }

        var period = SelectedPeriod!.Value;

        // Default to the current year on first visit to a sub-yearly
        // tab (no ?year= at all) rather than dumping every year of
        // history into one wide table - same reasoning as Reports/Index.
        if (ShowYearPicker && SelectedYear is null && !Request.Query.ContainsKey("year"))
            SelectedYear = DateTime.Today.Year;

        var yearFilter = period == ReportPeriod.Yearly ? null : SelectedYear;
        var flatRows = (await _reportService.GetOfficialTotalsAsync(period, yearFilter)).ToList();

        BuildPivot(flatRows);
        await LoadBudgetBreakdownAsync(period, yearFilter);
    }

    private async Task LoadBudgetBreakdownAsync(ReportPeriod? period, int? yearFilter)
    {
        var lines = (await _reportService.GetOfficialBudgetLineTotalsAsync(period, yearFilter)).ToList();

        BudgetBreakdownByPeriod = lines
            .GroupBy(l => (l.OfficialID, l.PeriodLabel))
            .ToDictionary(g => g.Key, g => g.ToList());

        // Re-aggregated across whatever periods came back, ignoring
        // PeriodLabel - for ITD this is a no-op (every row is already
        // labelled "ITD"), for the pivot table it collapses all period
        // buckets into the row's overall Total breakdown.
        BudgetBreakdownTotal = lines
            .GroupBy(l => l.OfficialID)
            .ToDictionary(
                g => g.Key,
                g => g.GroupBy(l => l.Category_Name)
                      .Select(cg => new OfficialBudgetLineTotal
                      {
                          OfficialID = g.Key,
                          PeriodLabel = "Total",
                          Category_Name = cg.Key,
                          TotalPaid = cg.Sum(l => l.TotalPaid)
                      })
                      .OrderByDescending(l => l.TotalPaid)
                      .ToList());
    }

    private void BuildPivot(List<OfficialPeriodTotal> flatRows)
    {
        // flatRows is already ordered by (Year, SortKey) from the SQL,
        // so distinct PeriodLabels come out in the right left-to-right
        // column order for free.
        PeriodColumns = flatRows.Select(r => r.PeriodLabel).Distinct().ToList();

        Rows = flatRows
            .GroupBy(r => (r.OfficialID, r.FullName, r.Role))
            .Select(g => new OfficialPivotRow(
                g.Key.OfficialID, g.Key.FullName, g.Key.Role,
                g.ToDictionary(r => r.PeriodLabel, r => r.TotalPaid),
                g.Sum(r => r.TotalPaid)))
            .OrderByDescending(r => r.Total)
            .ToList();

        ColumnTotals = PeriodColumns.ToDictionary(
            p => p,
            p => flatRows.Where(r => r.PeriodLabel == p).Sum(r => r.TotalPaid));

        GrandTotal = Rows.Sum(r => r.Total);
    }

    // Two-sheet workbook: "Summary" is exactly the pivot (or ITD ranking)
    // shown on screen; "Detail" is every underlying voucher payment that
    // rolls up into it, so every summary figure is traceable back to the
    // vouchers behind it. Not a live Excel PivotTable - Detail is a
    // clean flat table, so anyone who wants one can select it and use
    // Excel's own Insert > PivotTable rather than this app maintaining
    // PivotCache/PivotTable XML by hand.
    public async Task<IActionResult> OnGetExportXlsxAsync()
    {
        using var workbook = new XLWorkbook();

        if (IsItd)
        {
            var lifetime = (await _reportService.GetOfficialLifetimeTotalsAsync()).ToList();
            var grandTotal = lifetime.Sum(r => r.TotalPaid);

            var summary = workbook.Worksheets.Add("Summary");
            summary.Cell(1, 1).Value = "Official";
            summary.Cell(1, 2).Value = "Role";
            summary.Cell(1, 3).Value = "Total Paid (ITD)";

            var row = 2;
            foreach (var r in lifetime)
            {
                summary.Cell(row, 1).Value = r.FullName;
                summary.Cell(row, 2).Value = r.Role;
                summary.Cell(row, 3).Value = r.TotalPaid;
                row++;
            }
            summary.Cell(row, 1).Value = "Total";
            summary.Cell(row, 3).Value = grandTotal;
            summary.Range(1, 1, 1, 3).Style.Font.Bold = true;
            summary.Range(row, 1, row, 3).Style.Font.Bold = true;
            summary.Column(3).Style.NumberFormat.Format = "#,##0.00";
            summary.Columns().AdjustToContents();

            AddDetailSheet(workbook, await _reportService.GetOfficialPaymentDetailAsync(null, null));

            return SaveWorkbook(workbook, "OfficialTotals_ITD.xlsx");
        }

        var period = SelectedPeriod!.Value;
        var yearFilter = period == ReportPeriod.Yearly ? null : SelectedYear;
        var flatRows = (await _reportService.GetOfficialTotalsAsync(period, yearFilter)).ToList();
        BuildPivot(flatRows);

        var summarySheet = workbook.Worksheets.Add("Summary");
        summarySheet.Cell(1, 1).Value = "Official";
        summarySheet.Cell(1, 2).Value = "Role";
        var col = 3;
        foreach (var p in PeriodColumns)
        {
            summarySheet.Cell(1, col).Value = p;
            col++;
        }
        var totalCol = col;
        summarySheet.Cell(1, totalCol).Value = "Total";

        var rowIdx = 2;
        foreach (var r in Rows)
        {
            summarySheet.Cell(rowIdx, 1).Value = r.FullName;
            summarySheet.Cell(rowIdx, 2).Value = r.Role;
            col = 3;
            foreach (var p in PeriodColumns)
            {
                if (r.ByPeriod.TryGetValue(p, out var v))
                    summarySheet.Cell(rowIdx, col).Value = v;
                col++;
            }
            summarySheet.Cell(rowIdx, totalCol).Value = r.Total;
            rowIdx++;
        }
        summarySheet.Cell(rowIdx, 1).Value = "Total";
        col = 3;
        foreach (var p in PeriodColumns)
        {
            summarySheet.Cell(rowIdx, col).Value = ColumnTotals[p];
            col++;
        }
        summarySheet.Cell(rowIdx, totalCol).Value = GrandTotal;

        summarySheet.Range(1, 1, 1, totalCol).Style.Font.Bold = true;
        summarySheet.Range(rowIdx, 1, rowIdx, totalCol).Style.Font.Bold = true;
        summarySheet.Range(2, 3, rowIdx, totalCol).Style.NumberFormat.Format = "#,##0.00";
        summarySheet.Columns().AdjustToContents();

        AddDetailSheet(workbook, await _reportService.GetOfficialPaymentDetailAsync(period, yearFilter));

        return SaveWorkbook(workbook, $"OfficialTotals_{period}_{SelectedYear?.ToString() ?? "AllYears"}.xlsx");
    }

    private static void AddDetailSheet(XLWorkbook workbook, IEnumerable<OfficialPaymentDetail> rows)
    {
        var detail = workbook.Worksheets.Add("Detail");
        string[] headers = { "Official", "Role", "Period", "Voucher", "Voucher Date", "Description", "Payment", "Payment Date", "Amount" };
        for (var i = 0; i < headers.Length; i++)
            detail.Cell(1, i + 1).Value = headers[i];
        detail.Range(1, 1, 1, headers.Length).Style.Font.Bold = true;

        var row = 2;
        foreach (var d in rows)
        {
            detail.Cell(row, 1).Value = d.FullName;
            detail.Cell(row, 2).Value = d.Role;
            detail.Cell(row, 3).Value = d.PeriodLabel;
            detail.Cell(row, 4).Value = d.Voucher_ID;
            detail.Cell(row, 5).Value = d.VoucherDate;
            detail.Cell(row, 6).Value = d.Description;
            detail.Cell(row, 7).Value = d.Payment_ID;
            detail.Cell(row, 8).Value = d.Payment_Date;
            detail.Cell(row, 9).Value = d.Amount;
            row++;
        }

        detail.Column(5).Style.DateFormat.Format = "yyyy-mm-dd";
        detail.Column(8).Style.DateFormat.Format = "yyyy-mm-dd";
        detail.Column(9).Style.NumberFormat.Format = "#,##0.00";
        detail.Columns().AdjustToContents();

        // AutoFilter, not a real PivotTable - lets Excel users sort/filter
        // the detail immediately, and it's exactly the clean source range
        // Insert > PivotTable expects if they want a live pivot of their own.
        if (row > 2)
            detail.Range(1, 1, row - 1, headers.Length).SetAutoFilter();
    }

    private static FileContentResult SaveWorkbook(XLWorkbook workbook, string fileName)
    {
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return new FileContentResult(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
        {
            FileDownloadName = fileName
        };
    }
}
