-- No USE statement on purpose: run this with the target database chosen
-- explicitly (sqlcmd -d UASU_Finance_Test, or the SSMS database picker),
-- so it can't land on the live database by accident. Apply it to
-- UASU_Finance_Test first; run it on UASU_Finance_Web only once signed
-- off in dev, and take a backup before you do.

-- ====================================================================
-- A debt can span several days (e.g. a three-day meeting or a week of
-- strike activity).
--
-- Debt_Register only had Date_Incurred. This adds an optional
-- Date_Incurred_To for the last day; it stays NULL for a one-day debt.
-- Date_Incurred keeps meaning "first day", so ageing and every existing
-- report that reads it behave exactly as before.
--
-- DebtSummary is re-created with the same columns plus Date_Incurred_To
-- on the end, so nothing that reads it by name changes.
-- ====================================================================

-- --------------------------------------------------------------------
-- STEP 1: The column, never before the first day
-- --------------------------------------------------------------------
ALTER TABLE Debt_Register ADD Date_Incurred_To DATE NULL;
GO

ALTER TABLE Debt_Register ADD CONSTRAINT CK_Debt_DateRange
    CHECK (Date_Incurred_To IS NULL OR Date_Incurred_To >= Date_Incurred);
GO

-- --------------------------------------------------------------------
-- STEP 2: DebtSummary - as in SQL/031, plus Date_Incurred_To
-- --------------------------------------------------------------------
CREATE OR ALTER VIEW DebtSummary AS
SELECT
    d.Debt_ID,
    d.Creditor_Type,
    d.Official_Link,
    d.Supplier_Link,
    d.Date_Incurred,
    d.Invoice_No,
    d.Description,
    d.Total_Owed,
    ISNULL(a.Allocated, 0) AS TotalAllocated,
    d.Total_Owed - ISNULL(a.Allocated, 0) AS Balance,
    CASE
        WHEN d.Is_Settled = 1 THEN 'Settled'
        WHEN ISNULL(a.Allocated, 0) = 0 THEN 'Unpaid'
        ELSE 'Partially Paid'
    END AS SettlementStatus,
    d.Date_Incurred_To
FROM Debt_Register d
LEFT JOIN dbo.fn_DebtAllocations('9999-12-31') a ON a.Debt_ID = d.Debt_ID;
GO
