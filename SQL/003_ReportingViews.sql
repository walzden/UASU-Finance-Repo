USE UASU_Finance_Web;  -- adjust if your database name differs
GO

-- ====================================================================
-- Income/Expense summary views, one per reporting period.
-- All four share the same output shape (Year, SortKey, PeriodLabel,
-- TotalIncome, TotalExpense, NetBalance) so the app can query any of
-- them with one identical method. Based on PaymentAllocations, not
-- Vouchers.Amount - this reports money that actually moved, not
-- money that was merely raised on a voucher and might still be
-- pending or partially paid.
-- ====================================================================

CREATE VIEW IncomeExpenseByMonth AS
SELECT
    YEAR(v.VoucherDate) AS Year,
    MONTH(v.VoucherDate) AS SortKey,
    DATENAME(MONTH, v.VoucherDate) + ' ' + CAST(YEAR(v.VoucherDate) AS VARCHAR(4)) AS PeriodLabel,
    SUM(CASE WHEN v.Transaction_Type = 'Income' THEN pa.Allocated_Amount ELSE 0 END) AS TotalIncome,
    SUM(CASE WHEN v.Transaction_Type = 'Expense' THEN pa.Allocated_Amount ELSE 0 END) AS TotalExpense,
    SUM(CASE WHEN v.Transaction_Type = 'Income' THEN pa.Allocated_Amount ELSE 0 END)
      - SUM(CASE WHEN v.Transaction_Type = 'Expense' THEN pa.Allocated_Amount ELSE 0 END) AS NetBalance
FROM Vouchers v
INNER JOIN PaymentAllocations pa ON v.Voucher_ID = pa.Voucher_ID
GROUP BY YEAR(v.VoucherDate), MONTH(v.VoucherDate), DATENAME(MONTH, v.VoucherDate);
GO

CREATE VIEW IncomeExpenseByQuarter AS
SELECT
    YEAR(v.VoucherDate) AS Year,
    DATEPART(QUARTER, v.VoucherDate) AS SortKey,
    'Q' + CAST(DATEPART(QUARTER, v.VoucherDate) AS VARCHAR(1)) + ' ' + CAST(YEAR(v.VoucherDate) AS VARCHAR(4)) AS PeriodLabel,
    SUM(CASE WHEN v.Transaction_Type = 'Income' THEN pa.Allocated_Amount ELSE 0 END) AS TotalIncome,
    SUM(CASE WHEN v.Transaction_Type = 'Expense' THEN pa.Allocated_Amount ELSE 0 END) AS TotalExpense,
    SUM(CASE WHEN v.Transaction_Type = 'Income' THEN pa.Allocated_Amount ELSE 0 END)
      - SUM(CASE WHEN v.Transaction_Type = 'Expense' THEN pa.Allocated_Amount ELSE 0 END) AS NetBalance
FROM Vouchers v
INNER JOIN PaymentAllocations pa ON v.Voucher_ID = pa.Voucher_ID
GROUP BY YEAR(v.VoucherDate), DATEPART(QUARTER, v.VoucherDate);
GO

CREATE VIEW IncomeExpenseByHalfYear AS
SELECT
    YEAR(v.VoucherDate) AS Year,
    CASE WHEN MONTH(v.VoucherDate) <= 6 THEN 1 ELSE 2 END AS SortKey,
    'H' + CAST(CASE WHEN MONTH(v.VoucherDate) <= 6 THEN 1 ELSE 2 END AS VARCHAR(1))
        + ' ' + CAST(YEAR(v.VoucherDate) AS VARCHAR(4)) AS PeriodLabel,
    SUM(CASE WHEN v.Transaction_Type = 'Income' THEN pa.Allocated_Amount ELSE 0 END) AS TotalIncome,
    SUM(CASE WHEN v.Transaction_Type = 'Expense' THEN pa.Allocated_Amount ELSE 0 END) AS TotalExpense,
    SUM(CASE WHEN v.Transaction_Type = 'Income' THEN pa.Allocated_Amount ELSE 0 END)
      - SUM(CASE WHEN v.Transaction_Type = 'Expense' THEN pa.Allocated_Amount ELSE 0 END) AS NetBalance
FROM Vouchers v
INNER JOIN PaymentAllocations pa ON v.Voucher_ID = pa.Voucher_ID
GROUP BY YEAR(v.VoucherDate), CASE WHEN MONTH(v.VoucherDate) <= 6 THEN 1 ELSE 2 END;
GO

CREATE VIEW IncomeExpenseByYear AS
SELECT
    YEAR(v.VoucherDate) AS Year,
    1 AS SortKey,
    CAST(YEAR(v.VoucherDate) AS VARCHAR(4)) AS PeriodLabel,
    SUM(CASE WHEN v.Transaction_Type = 'Income' THEN pa.Allocated_Amount ELSE 0 END) AS TotalIncome,
    SUM(CASE WHEN v.Transaction_Type = 'Expense' THEN pa.Allocated_Amount ELSE 0 END) AS TotalExpense,
    SUM(CASE WHEN v.Transaction_Type = 'Income' THEN pa.Allocated_Amount ELSE 0 END)
      - SUM(CASE WHEN v.Transaction_Type = 'Expense' THEN pa.Allocated_Amount ELSE 0 END) AS NetBalance
FROM Vouchers v
INNER JOIN PaymentAllocations pa ON v.Voucher_ID = pa.Voucher_ID
GROUP BY YEAR(v.VoucherDate);
GO
