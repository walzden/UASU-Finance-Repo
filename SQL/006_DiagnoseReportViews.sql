USE UASU_Finance_Web;  -- adjust if your database name differs
GO

-- How many rows does each view actually have, with no year filter?
SELECT 'IncomeExpenseByMonth' AS ViewName, COUNT(*) AS RowCount FROM IncomeExpenseByMonth
UNION ALL
SELECT 'IncomeExpenseByQuarter', COUNT(*) FROM IncomeExpenseByQuarter
UNION ALL
SELECT 'IncomeExpenseByHalfYear', COUNT(*) FROM IncomeExpenseByHalfYear
UNION ALL
SELECT 'IncomeExpenseByYear', COUNT(*) FROM IncomeExpenseByYear;

-- What years does each view actually contain?
SELECT 'Month' AS Period, Year, COUNT(*) AS Rows FROM IncomeExpenseByMonth GROUP BY Year
UNION ALL
SELECT 'Quarter', Year, COUNT(*) FROM IncomeExpenseByQuarter GROUP BY Year
UNION ALL
SELECT 'HalfYear', Year, COUNT(*) FROM IncomeExpenseByHalfYear GROUP BY Year
UNION ALL
SELECT 'Year', Year, COUNT(*) FROM IncomeExpenseByYear GROUP BY Year
ORDER BY Period, Year;

-- What does the current page's actual query return for THIS year, for each period?
DECLARE @ThisYear INT = YEAR(GETDATE());
SELECT 'Month' AS Period, * FROM IncomeExpenseByMonth WHERE Year = @ThisYear
UNION ALL
SELECT 'Quarter', Year, SortKey, PeriodLabel, TotalIncome, TotalExpense, NetBalance FROM IncomeExpenseByQuarter WHERE Year = @ThisYear
UNION ALL
SELECT 'HalfYear', Year, SortKey, PeriodLabel, TotalIncome, TotalExpense, NetBalance FROM IncomeExpenseByHalfYear WHERE Year = @ThisYear
UNION ALL
SELECT 'Year', Year, SortKey, PeriodLabel, TotalIncome, TotalExpense, NetBalance FROM IncomeExpenseByYear WHERE Year = @ThisYear;
