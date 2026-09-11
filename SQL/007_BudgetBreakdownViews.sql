USE UASU_Finance_Web;  -- adjust if your database name differs
GO

-- ====================================================================
-- Budget-code breakdown of income/expense, one view per reporting
-- period - mirrors IncomeExpenseByMonth/Quarter/HalfYear/Year exactly,
-- just with Ref_BudgetCodes added to the GROUP BY. Same PeriodLabel/
-- SortKey shape as the originals, plus Budget_ID/Category_Name/
-- Category_Type, so the app can query "the detail behind this period
-- total" with one extra column set rather than a different shape.
-- ====================================================================

CREATE VIEW IncomeExpenseByMonthAndBudget AS
SELECT
    YEAR(v.VoucherDate) AS Year,
    MONTH(v.VoucherDate) AS SortKey,
    DATENAME(MONTH, v.VoucherDate) + ' ' + CAST(YEAR(v.VoucherDate) AS VARCHAR(4)) AS PeriodLabel,
    bc.Budget_ID, bc.Category_Name, bc.Category_Type,
    SUM(CASE WHEN v.Transaction_Type = 'Income' THEN pa.Allocated_Amount ELSE 0 END) AS TotalIncome,
    SUM(CASE WHEN v.Transaction_Type = 'Expense' THEN pa.Allocated_Amount ELSE 0 END) AS TotalExpense,
    SUM(CASE WHEN v.Transaction_Type = 'Income' THEN pa.Allocated_Amount ELSE 0 END)
      - SUM(CASE WHEN v.Transaction_Type = 'Expense' THEN pa.Allocated_Amount ELSE 0 END) AS NetBalance
FROM Vouchers v
INNER JOIN PaymentAllocations pa ON v.Voucher_ID = pa.Voucher_ID
INNER JOIN Ref_BudgetCodes bc ON bc.Budget_ID = v.Budget_Link
GROUP BY YEAR(v.VoucherDate), MONTH(v.VoucherDate), DATENAME(MONTH, v.VoucherDate),
         bc.Budget_ID, bc.Category_Name, bc.Category_Type;
GO

CREATE VIEW IncomeExpenseByQuarterAndBudget AS
SELECT
    YEAR(v.VoucherDate) AS Year,
    DATEPART(QUARTER, v.VoucherDate) AS SortKey,
    'Q' + CAST(DATEPART(QUARTER, v.VoucherDate) AS VARCHAR(1)) + ' ' + CAST(YEAR(v.VoucherDate) AS VARCHAR(4)) AS PeriodLabel,
    bc.Budget_ID, bc.Category_Name, bc.Category_Type,
    SUM(CASE WHEN v.Transaction_Type = 'Income' THEN pa.Allocated_Amount ELSE 0 END) AS TotalIncome,
    SUM(CASE WHEN v.Transaction_Type = 'Expense' THEN pa.Allocated_Amount ELSE 0 END) AS TotalExpense,
    SUM(CASE WHEN v.Transaction_Type = 'Income' THEN pa.Allocated_Amount ELSE 0 END)
      - SUM(CASE WHEN v.Transaction_Type = 'Expense' THEN pa.Allocated_Amount ELSE 0 END) AS NetBalance
FROM Vouchers v
INNER JOIN PaymentAllocations pa ON v.Voucher_ID = pa.Voucher_ID
INNER JOIN Ref_BudgetCodes bc ON bc.Budget_ID = v.Budget_Link
GROUP BY YEAR(v.VoucherDate), DATEPART(QUARTER, v.VoucherDate),
         bc.Budget_ID, bc.Category_Name, bc.Category_Type;
GO

CREATE VIEW IncomeExpenseByHalfYearAndBudget AS
SELECT
    YEAR(v.VoucherDate) AS Year,
    CASE WHEN MONTH(v.VoucherDate) <= 6 THEN 1 ELSE 2 END AS SortKey,
    'H' + CAST(CASE WHEN MONTH(v.VoucherDate) <= 6 THEN 1 ELSE 2 END AS VARCHAR(1))
        + ' ' + CAST(YEAR(v.VoucherDate) AS VARCHAR(4)) AS PeriodLabel,
    bc.Budget_ID, bc.Category_Name, bc.Category_Type,
    SUM(CASE WHEN v.Transaction_Type = 'Income' THEN pa.Allocated_Amount ELSE 0 END) AS TotalIncome,
    SUM(CASE WHEN v.Transaction_Type = 'Expense' THEN pa.Allocated_Amount ELSE 0 END) AS TotalExpense,
    SUM(CASE WHEN v.Transaction_Type = 'Income' THEN pa.Allocated_Amount ELSE 0 END)
      - SUM(CASE WHEN v.Transaction_Type = 'Expense' THEN pa.Allocated_Amount ELSE 0 END) AS NetBalance
FROM Vouchers v
INNER JOIN PaymentAllocations pa ON v.Voucher_ID = pa.Voucher_ID
INNER JOIN Ref_BudgetCodes bc ON bc.Budget_ID = v.Budget_Link
GROUP BY YEAR(v.VoucherDate), CASE WHEN MONTH(v.VoucherDate) <= 6 THEN 1 ELSE 2 END,
         bc.Budget_ID, bc.Category_Name, bc.Category_Type;
GO

CREATE VIEW IncomeExpenseByYearAndBudget AS
SELECT
    YEAR(v.VoucherDate) AS Year,
    1 AS SortKey,
    CAST(YEAR(v.VoucherDate) AS VARCHAR(4)) AS PeriodLabel,
    bc.Budget_ID, bc.Category_Name, bc.Category_Type,
    SUM(CASE WHEN v.Transaction_Type = 'Income' THEN pa.Allocated_Amount ELSE 0 END) AS TotalIncome,
    SUM(CASE WHEN v.Transaction_Type = 'Expense' THEN pa.Allocated_Amount ELSE 0 END) AS TotalExpense,
    SUM(CASE WHEN v.Transaction_Type = 'Income' THEN pa.Allocated_Amount ELSE 0 END)
      - SUM(CASE WHEN v.Transaction_Type = 'Expense' THEN pa.Allocated_Amount ELSE 0 END) AS NetBalance
FROM Vouchers v
INNER JOIN PaymentAllocations pa ON v.Voucher_ID = pa.Voucher_ID
INNER JOIN Ref_BudgetCodes bc ON bc.Budget_ID = v.Budget_Link
GROUP BY YEAR(v.VoucherDate), bc.Budget_ID, bc.Category_Name, bc.Category_Type;
GO
