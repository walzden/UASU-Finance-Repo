using Dapper;
using UASU_VoucherApprovals.Data;
using UASU_VoucherApprovals.Models;

namespace UASU_VoucherApprovals.Services;

public enum ReportPeriod { Monthly, Quarterly, HalfYearly, Yearly }

public interface IReportService
{
    Task<IEnumerable<PeriodSummary>> GetSummaryAsync(ReportPeriod period, int? year);
    Task<IEnumerable<BudgetBreakdownRow>> GetBudgetBreakdownAsync(ReportPeriod period, int? year);
    Task<IEnumerable<int>> GetAvailableYearsAsync();
    Task<IEnumerable<OfficialPeriodTotal>> GetOfficialTotalsAsync(ReportPeriod period, int? year);
    Task<IEnumerable<OfficialLifetimeTotal>> GetOfficialLifetimeTotalsAsync();
    Task<IEnumerable<OfficialPaymentDetail>> GetOfficialPaymentDetailAsync(ReportPeriod? period, int? year);
    Task<IEnumerable<OfficialBudgetLineTotal>> GetOfficialBudgetLineTotalsAsync(ReportPeriod? period, int? year);

    Task<IEnumerable<CashBookRow>> GetCashBookAsync(int? year);

    Task<BalanceSheetData> GetBalanceSheetAsync(int year, int month);
}

public class ReportService : IReportService
{
    private readonly IDbConnectionFactory _db;
    private readonly ICertificationService _certificationService;

    public ReportService(IDbConnectionFactory db, ICertificationService certificationService)
    {
        _db = db;
        _certificationService = certificationService;
    }

    // All four period views share the same column shape (Year, SortKey,
    // PeriodLabel, TotalIncome, TotalExpense, NetBalance), so the view
    // name is the only thing that varies between periods.
    private static string ViewNameFor(ReportPeriod period) => period switch
    {
        ReportPeriod.Monthly => "IncomeExpenseByMonth",
        ReportPeriod.Quarterly => "IncomeExpenseByQuarter",
        ReportPeriod.HalfYearly => "IncomeExpenseByHalfYear",
        ReportPeriod.Yearly => "IncomeExpenseByYear",
        _ => throw new ArgumentOutOfRangeException(nameof(period))
    };

    // The *AndBudget views mirror the ones above exactly, plus a
    // budget-code breakdown - same naming pattern, so this is just
    // ViewNameFor with a suffix rather than a second switch to keep
    // in sync by hand.
    private static string BreakdownViewNameFor(ReportPeriod period) => ViewNameFor(period) + "AndBudget";

    public async Task<IEnumerable<PeriodSummary>> GetSummaryAsync(ReportPeriod period, int? year)
    {
        using var conn = _db.CreateConnection();

        var viewName = ViewNameFor(period);

        // View name comes only from the fixed enum mapping above, never
        // from user input, so interpolating it here doesn't open up
        // SQL injection the way concatenating user input would.
        var sql = $@"
            SELECT Year, PeriodLabel, TotalIncome, TotalExpense, NetBalance
            FROM {viewName}
            WHERE (@Year IS NULL OR Year = @Year)
            ORDER BY Year DESC, SortKey DESC;";

        return await conn.QueryAsync<PeriodSummary>(sql, new { Year = year });
    }

    public async Task<IEnumerable<BudgetBreakdownRow>> GetBudgetBreakdownAsync(ReportPeriod period, int? year)
    {
        using var conn = _db.CreateConnection();

        var viewName = BreakdownViewNameFor(period);

        var sql = $@"
            SELECT PeriodLabel, Budget_ID, Category_Name, Category_Type, TotalIncome, TotalExpense, NetBalance
            FROM {viewName}
            WHERE (@Year IS NULL OR Year = @Year)
            ORDER BY Year DESC, SortKey DESC, Category_Name;";

        return await conn.QueryAsync<BudgetBreakdownRow>(sql, new { Year = year });
    }

    public async Task<IEnumerable<int>> GetAvailableYearsAsync()
    {
        using var conn = _db.CreateConnection();
        const string sql = "SELECT DISTINCT Year FROM IncomeExpenseByYear ORDER BY Year DESC;";
        return await conn.QueryAsync<int>(sql);
    }

    // Same naming pattern as ViewNameFor, one suffix over - kept as its
    // own switch rather than reusing ViewNameFor plus a prefix swap,
    // since "Official" isn't a mechanical transform of "IncomeExpense".
    private static string OfficialViewNameFor(ReportPeriod period) => period switch
    {
        ReportPeriod.Monthly => "OfficialTotalsByMonth",
        ReportPeriod.Quarterly => "OfficialTotalsByQuarter",
        ReportPeriod.HalfYearly => "OfficialTotalsByHalfYear",
        ReportPeriod.Yearly => "OfficialTotalsByYear",
        _ => throw new ArgumentOutOfRangeException(nameof(period))
    };

    public async Task<IEnumerable<OfficialPeriodTotal>> GetOfficialTotalsAsync(ReportPeriod period, int? year)
    {
        using var conn = _db.CreateConnection();

        var viewName = OfficialViewNameFor(period);

        // Ordered by (Year, SortKey) so the page can read off period
        // columns for the pivot in the right order without needing
        // SortKey itself in the result - same reasoning as GetSummaryAsync.
        var sql = $@"
            SELECT PeriodLabel, OfficialID, FullName, Role, TotalPaid
            FROM {viewName}
            WHERE (@Year IS NULL OR Year = @Year)
            ORDER BY Year, SortKey;";

        return await conn.QueryAsync<OfficialPeriodTotal>(sql, new { Year = year });
    }

    public async Task<IEnumerable<OfficialLifetimeTotal>> GetOfficialLifetimeTotalsAsync()
    {
        using var conn = _db.CreateConnection();

        const string sql = @"
            SELECT OfficialID, FullName, Role, TotalPaid
            FROM OfficialTotalsAllTime
            ORDER BY TotalPaid DESC;";

        return await conn.QueryAsync<OfficialLifetimeTotal>(sql);
    }

    // Same period-label expressions as the OfficialTotalsBy* views
    // (SQL/019), duplicated here rather than reused because those views
    // are pre-aggregated (SUM per official/period) and can't expose the
    // individual Voucher_ID/Payment_ID rows behind each cell - this
    // queries the base tables directly instead, at voucher-allocation
    // grain, for the Excel export's Detail sheet. Null period means the
    // Total (ITD) view, where every row is labelled "ITD" and no year
    // filter applies. The expression comes only from this fixed switch,
    // never from user input, so interpolating it into the SQL is safe.
    private static string PeriodLabelExpr(ReportPeriod? period) => period switch
    {
        null => "'ITD'",
        ReportPeriod.Monthly => "DATENAME(MONTH, v.VoucherDate) + ' ' + CAST(YEAR(v.VoucherDate) AS VARCHAR(4))",
        ReportPeriod.Quarterly => "'Q' + CAST(DATEPART(QUARTER, v.VoucherDate) AS VARCHAR(1)) + ' ' + CAST(YEAR(v.VoucherDate) AS VARCHAR(4))",
        ReportPeriod.HalfYearly => "'H' + CAST(CASE WHEN MONTH(v.VoucherDate) <= 6 THEN 1 ELSE 2 END AS VARCHAR(1)) + ' ' + CAST(YEAR(v.VoucherDate) AS VARCHAR(4))",
        ReportPeriod.Yearly => "CAST(YEAR(v.VoucherDate) AS VARCHAR(4))",
        _ => throw new ArgumentOutOfRangeException(nameof(period))
    };

    public async Task<IEnumerable<OfficialPaymentDetail>> GetOfficialPaymentDetailAsync(ReportPeriod? period, int? year)
    {
        using var conn = _db.CreateConnection();

        var sql = $@"
            SELECT o.OfficialID, o.FullName, o.Role,
                   {PeriodLabelExpr(period)} AS PeriodLabel,
                   v.Voucher_ID, v.VoucherDate, v.[Description],
                   p.Payment_ID, p.Payment_Date,
                   pa.Allocated_Amount AS Amount
            FROM Vouchers v
            INNER JOIN PaymentAllocations pa ON v.Voucher_ID = pa.Voucher_ID
            INNER JOIN Payments p ON p.Payment_ID = pa.Payment_ID
            INNER JOIN Ref_Officials o ON o.OfficialID = v.Official_Link
            WHERE v.Transaction_Type = 'Expense'
              AND (@Year IS NULL OR YEAR(v.VoucherDate) = @Year)
            ORDER BY o.FullName, v.VoucherDate;";

        return await conn.QueryAsync<OfficialPaymentDetail>(sql, new { Year = year });
    }

    // One row per (official, period bucket, budget line) - same
    // PeriodLabelExpr as GetOfficialPaymentDetailAsync (null period =
    // "ITD"), so grouping these by (OfficialID, PeriodLabel) reproduces
    // exactly one period cell's amount, and grouping by OfficialID alone
    // reproduces the row's overall Total. Backs the hover breakdown on
    // Reports/OfficialTotals; year is null for both the ITD tab and the
    // Yearly tab (neither has a year filter), same as the totals
    // queries themselves.
    public async Task<IEnumerable<OfficialBudgetLineTotal>> GetOfficialBudgetLineTotalsAsync(ReportPeriod? period, int? year)
    {
        using var conn = _db.CreateConnection();

        // For ITD, PeriodLabelExpr(null) is the literal 'ITD' - the same
        // for every row, so it doesn't need grouping, and SQL Server
        // actually rejects a GROUP BY expression that's a bare constant
        // ("Each GROUP BY expression must contain at least one column
        // that is not an outer reference"). Monthly/Quarterly/HalfYearly/
        // Yearly are real expressions derived from VoucherDate, so those
        // DO need to stay in GROUP BY.
        var periodGroupBy = period is null ? "" : $", {PeriodLabelExpr(period)}";

        var sql = $@"
            SELECT o.OfficialID,
                   {PeriodLabelExpr(period)} AS PeriodLabel,
                   ISNULL(bc.Category_Name, 'Uncategorized') AS Category_Name,
                   SUM(pa.Allocated_Amount) AS TotalPaid
            FROM Vouchers v
            INNER JOIN PaymentAllocations pa ON v.Voucher_ID = pa.Voucher_ID
            INNER JOIN Ref_Officials o ON o.OfficialID = v.Official_Link
            LEFT JOIN Ref_BudgetCodes bc ON bc.Budget_ID = v.Budget_Link
            WHERE v.Transaction_Type = 'Expense'
              AND (@Year IS NULL OR YEAR(v.VoucherDate) = @Year)
            GROUP BY o.OfficialID{periodGroupBy}, ISNULL(bc.Category_Name, 'Uncategorized')
            ORDER BY o.OfficialID, SUM(pa.Allocated_Amount) DESC;";

        return await conn.QueryAsync<OfficialBudgetLineTotal>(sql, new { Year = year });
    }

    // Mirrors a traditional two-column (Cash/Bank) cash book ledger -
    // same underlying rule CertificationService.GetBookBalancesAsync
    // uses to split Cash vs Bank (Payment_Mode = 'Cash' OR the payment
    // was funded from a withdrawal), just presented as a chronological
    // line-by-line ledger instead of a month-end summary. A bank
    // withdrawal is its own contra pair (Paid: Bank + Received: Cash
    // for the same amount, same line) rather than two separate rows,
    // matching how "C" entries read in the physical book.
    //
    // Fetches every row ever (no year filter in SQL) so the running
    // balance carried in C# stays correct across year boundaries, then
    // filters down to the requested year only for display.
    public async Task<IEnumerable<CashBookRow>> GetCashBookAsync(int? year)
    {
        using var conn = _db.CreateConnection();

        const string sql = @"
            SELECT EntryDate, Item, VoucherOrReceiptNo, ReceivedCash, ReceivedBank, PaidCash, PaidBank
            FROM (
                SELECT As_Of_Date AS EntryDate, 'Balance b/f' AS Item, CAST(NULL AS NVARCHAR(40)) AS VoucherOrReceiptNo,
                       Cash_Balance AS ReceivedCash, Bank_Balance AS ReceivedBank, 0 AS PaidCash, 0 AS PaidBank, 0 AS SourceOrder
                FROM OpeningBalance WHERE Id = 1

                UNION ALL

                -- One line per (date, budget category) rather than per
                -- voucher - several same-day vouchers under one budget
                -- code collapse into a single row, matching how the
                -- physical book groups same-day entries. VoucherOrReceiptNo
                -- becomes a count instead of one specific Voucher_ID once
                -- more than one voucher is behind the line.
                SELECT EntryDate, Item,
                       CASE WHEN COUNT(*) = 1 THEN MAX(Voucher_ID) ELSE CAST(COUNT(*) AS NVARCHAR(10)) + ' vouchers' END,
                       SUM(CASE WHEN IsCashBucket = 1 THEN Allocated_Amount ELSE 0 END),
                       SUM(CASE WHEN IsCashBucket = 0 THEN Allocated_Amount ELSE 0 END),
                       0, 0, 1
                FROM (
                    -- Per-row cash/bank classification computed first
                    -- (same rule as CertificationService.GetBookBalancesAsync) -
                    -- SQL Server won't allow the EXISTS subquery this
                    -- depends on directly inside a SUM() once GROUP BY
                    -- is involved, so it has to be resolved per-row here
                    -- before the outer query aggregates it.
                    SELECT p.Payment_Date AS EntryDate,
                           ISNULL(bc.Category_Name, 'Uncategorized') AS Item,
                           v.Voucher_ID, pa.Allocated_Amount,
                           CASE WHEN p.Payment_Mode = 'Cash' OR EXISTS (SELECT 1 FROM WithdrawalPayments wp WHERE wp.Payment_ID = p.Payment_ID) THEN 1 ELSE 0 END AS IsCashBucket
                    FROM Vouchers v
                    INNER JOIN PaymentAllocations pa ON pa.Voucher_ID = v.Voucher_ID
                    INNER JOIN Payments p ON p.Payment_ID = pa.Payment_ID
                    LEFT JOIN Ref_BudgetCodes bc ON bc.Budget_ID = v.Budget_Link
                    WHERE v.Transaction_Type = 'Income'
                ) src
                GROUP BY EntryDate, Item

                UNION ALL

                SELECT EntryDate, Item,
                       CASE WHEN COUNT(*) = 1 THEN MAX(Voucher_ID) ELSE CAST(COUNT(*) AS NVARCHAR(10)) + ' vouchers' END,
                       0, 0,
                       SUM(CASE WHEN IsCashBucket = 1 THEN Allocated_Amount ELSE 0 END),
                       SUM(CASE WHEN IsCashBucket = 0 THEN Allocated_Amount ELSE 0 END),
                       2
                FROM (
                    SELECT p.Payment_Date AS EntryDate,
                           ISNULL(bc.Category_Name, 'Uncategorized') AS Item,
                           v.Voucher_ID, pa.Allocated_Amount,
                           CASE WHEN p.Payment_Mode = 'Cash' OR EXISTS (SELECT 1 FROM WithdrawalPayments wp WHERE wp.Payment_ID = p.Payment_ID) THEN 1 ELSE 0 END AS IsCashBucket
                    FROM Vouchers v
                    INNER JOIN PaymentAllocations pa ON pa.Voucher_ID = v.Voucher_ID
                    INNER JOIN Payments p ON p.Payment_ID = pa.Payment_ID
                    LEFT JOIN Ref_BudgetCodes bc ON bc.Budget_ID = v.Budget_Link
                    WHERE v.Transaction_Type = 'Expense'
                ) src
                GROUP BY EntryDate, Item

                UNION ALL

                -- Contra: cash pulled out of the bank into hand - same
                -- amount leaves Bank and enters Cash on one line.
                SELECT Withdrawal_Date, 'Cash Withdrawal', Withdrawal_ID, Amount, 0, 0, Amount, 3
                FROM BankWithdrawals

                UNION ALL

                -- Same-day fees of the same type (e.g. several M-PESA Fee
                -- charges on one date) collapse into one line, same
                -- reasoning as the voucher grouping above.
                SELECT p.Payment_Date, pc.Charge_Type,
                       CASE WHEN COUNT(*) = 1 THEN MAX(pc.Charge_ID) ELSE CAST(COUNT(*) AS NVARCHAR(10)) + ' charges' END,
                       0, 0, 0, SUM(pc.Charge_Amount), 4
                FROM PaymentCharges pc
                INNER JOIN Payments p ON p.Payment_ID = pc.Payment_ID
                WHERE NOT EXISTS (SELECT 1 FROM WithdrawalPayments wp WHERE wp.Payment_ID = pc.Payment_ID)
                GROUP BY p.Payment_Date, pc.Charge_Type

                UNION ALL

                SELECT p.Payment_Date, pc.Charge_Type,
                       CASE WHEN COUNT(*) = 1 THEN MAX(pc.Charge_ID) ELSE CAST(COUNT(*) AS NVARCHAR(10)) + ' charges' END,
                       0, 0, SUM(pc.Charge_Amount), 0, 4
                FROM PaymentCharges pc
                INNER JOIN Payments p ON p.Payment_ID = pc.Payment_ID
                WHERE EXISTS (SELECT 1 FROM WithdrawalPayments wp WHERE wp.Payment_ID = pc.Payment_ID)
                GROUP BY p.Payment_Date, pc.Charge_Type

                UNION ALL

                SELECT Charge_Date, Charge_Type, Charge_ID, 0, 0, 0, Charge_Amount, 5
                FROM AccountCharges
            ) x
            ORDER BY EntryDate, SourceOrder;";

        var allRows = (await conn.QueryAsync<CashBookRow>(sql)).ToList();

        decimal runningCash = 0, runningBank = 0;
        foreach (var row in allRows)
        {
            runningCash += row.ReceivedCash - row.PaidCash;
            runningBank += row.ReceivedBank - row.PaidBank;
            row.RunningCashBalance = runningCash;
            row.RunningBankBalance = runningBank;
        }

        return year.HasValue ? allRows.Where(r => r.EntryDate.Year == year.Value).ToList() : allRows;
    }

    // Cash/Bank come straight from CertificationService - same cumulative-
    // to-month-end logic already trusted for monthly certification, so
    // this can't disagree with what that report says. Accounts Payable
    // has no equivalent existing "as at a date" source (DebtSummary has
    // no cutoff - it's always "as of right now"), so it's computed here
    // directly: a debt counts only if incurred by AsOfDate, and only the
    // portion of it still unpaid by payments made by AsOfDate. The paid
    // portion comes from fn_DebtAllocations (SQL/031), the same function
    // DebtSummary uses, so it covers vouchers that pay several debts and
    // cannot disagree with the Debt Status report - here with the date guard
    // GetBookBalancesAsync uses.
    public async Task<BalanceSheetData> GetBalanceSheetAsync(int year, int month)
    {
        var (cash, bank) = await _certificationService.GetBookBalancesAsync(year, month);
        var asOfDate = new DateTime(year, month, 1).AddMonths(1).AddDays(-1);

        using var conn = _db.CreateConnection();
        const string sql = @"
            SELECT ISNULL(SUM(d.Total_Owed - ISNULL(paid.Allocated, 0)), 0)
            FROM Debt_Register d
            LEFT JOIN dbo.fn_DebtAllocations(@AsOfDate) paid ON paid.Debt_ID = d.Debt_ID
            WHERE d.Date_Incurred <= @AsOfDate;";

        var accountsPayable = await conn.ExecuteScalarAsync<decimal>(sql, new { AsOfDate = asOfDate });

        return new BalanceSheetData
        {
            AsOfDate = asOfDate,
            CashInHand = cash,
            BankBalance = bank,
            AccountsPayable = accountsPayable
        };
    }
}
