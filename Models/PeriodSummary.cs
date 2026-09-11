namespace UASU_VoucherApprovals.Models;

// One row from any of the four IncomeExpenseBy* views - same shape
// regardless of period, so one method/model handles all of them.
public class PeriodSummary
{
    public int Year { get; set; }
    public string PeriodLabel { get; set; } = string.Empty;
    public decimal TotalIncome { get; set; }
    public decimal TotalExpense { get; set; }
    public decimal NetBalance { get; set; }
}

// One row of the budget-code breakdown behind a period's total - same
// shape as PeriodSummary plus which budget category it belongs to.
public class BudgetBreakdownRow
{
    public string PeriodLabel { get; set; } = string.Empty;
    public string Budget_ID { get; set; } = string.Empty;
    public string Category_Name { get; set; } = string.Empty;
    public string Category_Type { get; set; } = string.Empty;
    public decimal TotalIncome { get; set; }
    public decimal TotalExpense { get; set; }
    public decimal NetBalance { get; set; }
}

// One official's total for a single period bucket, from any of the
// four OfficialTotalsBy* views - the SQL already orders by (Year,
// SortKey), so the page can build the pivot's period columns just by
// taking distinct PeriodLabels in row order, without needing SortKey
// itself as a property here.
public class OfficialPeriodTotal
{
    public string PeriodLabel { get; set; } = string.Empty;
    public string OfficialID { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Role { get; set; }
    public decimal TotalPaid { get; set; }
}

// One official's lifetime (inception-to-date) total - no period
// dimension, backs the report's Total (ITD) view.
public class OfficialLifetimeTotal
{
    public string OfficialID { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Role { get; set; }
    public decimal TotalPaid { get; set; }
}

// One official's total for a single budget line within a single period
// bucket (or "ITD" for the lifetime view) - backs the Official Totals
// hover breakdown. Grouping these by (OfficialID, PeriodLabel) and
// summing TotalPaid reproduces that exact period cell's own amount;
// grouping by OfficialID alone (ignoring PeriodLabel) reproduces the
// row's overall Total.
public class OfficialBudgetLineTotal
{
    public string OfficialID { get; set; } = string.Empty;
    public string PeriodLabel { get; set; } = string.Empty;
    public string Category_Name { get; set; } = string.Empty;
    public decimal TotalPaid { get; set; }
}

// One voucher-level payment to an official - the row-level data behind
// every cell in the Official Totals pivot (or the ITD ranking), used
// for the "Detail" sheet of its Excel export so the summary numbers
// are traceable back to the actual vouchers that sum to them.
public class OfficialPaymentDetail
{
    public string OfficialID { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Role { get; set; }
    public string PeriodLabel { get; set; } = string.Empty;
    public string Voucher_ID { get; set; } = string.Empty;
    public DateTime VoucherDate { get; set; }
    public string? Description { get; set; }
    public string Payment_ID { get; set; } = string.Empty;
    public DateTime Payment_Date { get; set; }
    public decimal Amount { get; set; }
}
