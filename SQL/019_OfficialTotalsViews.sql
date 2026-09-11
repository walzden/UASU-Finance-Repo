USE UASU_Finance_Web;  -- adjust if your database name differs
GO

-- ====================================================================
-- Total sums paid to each official, one view per reporting period -
-- mirrors IncomeExpenseByMonth/Quarter/HalfYear/Year exactly (same
-- Year/SortKey/PeriodLabel shape), grouped by Ref_Officials instead of
-- just summed, and scoped to Expense vouchers with Official_Link set -
-- this reports money paid OUT to an official, not money an official's
-- voucher happened to raise as Income (e.g. a reimbursement they made
-- to the union, which isn't "money paid to them"). Based on
-- PaymentAllocations, not Vouchers.Amount, same reasoning as the
-- Income/Expense views: this reports money that actually moved.
--
-- A fifth view, OfficialTotalsAllTime, has no period dimension at all -
-- it backs the report's "Total (ITD)" tab, a single lifetime total per
-- official rather than a period breakdown.
-- ====================================================================

CREATE VIEW OfficialTotalsByMonth AS
SELECT
    YEAR(v.VoucherDate) AS Year,
    MONTH(v.VoucherDate) AS SortKey,
    DATENAME(MONTH, v.VoucherDate) + ' ' + CAST(YEAR(v.VoucherDate) AS VARCHAR(4)) AS PeriodLabel,
    o.OfficialID, o.FullName, o.Role,
    SUM(pa.Allocated_Amount) AS TotalPaid
FROM Vouchers v
INNER JOIN PaymentAllocations pa ON v.Voucher_ID = pa.Voucher_ID
INNER JOIN Ref_Officials o ON o.OfficialID = v.Official_Link
WHERE v.Transaction_Type = 'Expense'
GROUP BY YEAR(v.VoucherDate), MONTH(v.VoucherDate), DATENAME(MONTH, v.VoucherDate),
         o.OfficialID, o.FullName, o.Role;
GO

CREATE VIEW OfficialTotalsByQuarter AS
SELECT
    YEAR(v.VoucherDate) AS Year,
    DATEPART(QUARTER, v.VoucherDate) AS SortKey,
    'Q' + CAST(DATEPART(QUARTER, v.VoucherDate) AS VARCHAR(1)) + ' ' + CAST(YEAR(v.VoucherDate) AS VARCHAR(4)) AS PeriodLabel,
    o.OfficialID, o.FullName, o.Role,
    SUM(pa.Allocated_Amount) AS TotalPaid
FROM Vouchers v
INNER JOIN PaymentAllocations pa ON v.Voucher_ID = pa.Voucher_ID
INNER JOIN Ref_Officials o ON o.OfficialID = v.Official_Link
WHERE v.Transaction_Type = 'Expense'
GROUP BY YEAR(v.VoucherDate), DATEPART(QUARTER, v.VoucherDate),
         o.OfficialID, o.FullName, o.Role;
GO

CREATE VIEW OfficialTotalsByHalfYear AS
SELECT
    YEAR(v.VoucherDate) AS Year,
    CASE WHEN MONTH(v.VoucherDate) <= 6 THEN 1 ELSE 2 END AS SortKey,
    'H' + CAST(CASE WHEN MONTH(v.VoucherDate) <= 6 THEN 1 ELSE 2 END AS VARCHAR(1))
        + ' ' + CAST(YEAR(v.VoucherDate) AS VARCHAR(4)) AS PeriodLabel,
    o.OfficialID, o.FullName, o.Role,
    SUM(pa.Allocated_Amount) AS TotalPaid
FROM Vouchers v
INNER JOIN PaymentAllocations pa ON v.Voucher_ID = pa.Voucher_ID
INNER JOIN Ref_Officials o ON o.OfficialID = v.Official_Link
WHERE v.Transaction_Type = 'Expense'
GROUP BY YEAR(v.VoucherDate), CASE WHEN MONTH(v.VoucherDate) <= 6 THEN 1 ELSE 2 END,
         o.OfficialID, o.FullName, o.Role;
GO

CREATE VIEW OfficialTotalsByYear AS
SELECT
    YEAR(v.VoucherDate) AS Year,
    1 AS SortKey,
    CAST(YEAR(v.VoucherDate) AS VARCHAR(4)) AS PeriodLabel,
    o.OfficialID, o.FullName, o.Role,
    SUM(pa.Allocated_Amount) AS TotalPaid
FROM Vouchers v
INNER JOIN PaymentAllocations pa ON v.Voucher_ID = pa.Voucher_ID
INNER JOIN Ref_Officials o ON o.OfficialID = v.Official_Link
WHERE v.Transaction_Type = 'Expense'
GROUP BY YEAR(v.VoucherDate), o.OfficialID, o.FullName, o.Role;
GO

CREATE VIEW OfficialTotalsAllTime AS
SELECT
    o.OfficialID, o.FullName, o.Role,
    SUM(pa.Allocated_Amount) AS TotalPaid
FROM Vouchers v
INNER JOIN PaymentAllocations pa ON v.Voucher_ID = pa.Voucher_ID
INNER JOIN Ref_Officials o ON o.OfficialID = v.Official_Link
WHERE v.Transaction_Type = 'Expense'
GROUP BY o.OfficialID, o.FullName, o.Role;
GO
