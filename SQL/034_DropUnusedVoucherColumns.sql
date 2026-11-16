-- No USE statement on purpose: run this with the target database chosen
-- explicitly (sqlcmd -d UASU_Finance_Test, or the SSMS database picker),
-- so it can't land on the live database by accident. Apply it to
-- UASU_Finance_Test first; run it on UASU_Finance_Web only once signed
-- off in dev, and take a backup before you do. Run AFTER 033.

-- ====================================================================
-- Drop three Vouchers columns nothing uses any more.
--
-- Deposit_Proof, Payer_Details and Rejection_Reason were carried over
-- from the old UASU_Finance_01 schema by SQL/004 and have never been read
-- or written by the app, a view, a function or a trigger since:
--   - rejection reasons live in Voucher_Approvals.Comments
--   - void reasons live in Vouchers.Void_Reason (SQL/033)
--   - payment references live in Payments.Reference_No
--
-- Any value still in them is copied to Vouchers_RetiredColumns first, so
-- nothing is lost. At the time of writing that is one row: the M-PESA
-- confirmation message in Deposit_Proof on legacy income voucher
-- UASU-MMU-0047/26.
--
-- SQL/004 (the one-off legacy import) names these columns, so it can't
-- be re-run as-is after this script - it isn't meant to be.
--
-- Each step is its own batch (GO); if a step errors, stop and send the
-- message rather than continuing.
-- ====================================================================

-- --------------------------------------------------------------------
-- STEP 1: Archive any values
-- --------------------------------------------------------------------
CREATE TABLE Vouchers_RetiredColumns (
    Voucher_ID NVARCHAR(20) NOT NULL
        CONSTRAINT FK_VouchersRetiredColumns_Voucher FOREIGN KEY REFERENCES Vouchers(Voucher_ID),
    Column_Name NVARCHAR(50) NOT NULL,
    Value NVARCHAR(255) NOT NULL,
    Archived_Date DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT PK_VouchersRetiredColumns PRIMARY KEY (Voucher_ID, Column_Name)
);
GO

INSERT INTO Vouchers_RetiredColumns (Voucher_ID, Column_Name, Value)
SELECT Voucher_ID, 'Deposit_Proof', Deposit_Proof FROM Vouchers WHERE NULLIF(LTRIM(RTRIM(Deposit_Proof)), '') IS NOT NULL
UNION ALL
SELECT Voucher_ID, 'Payer_Details', Payer_Details FROM Vouchers WHERE NULLIF(LTRIM(RTRIM(Payer_Details)), '') IS NOT NULL
UNION ALL
SELECT Voucher_ID, 'Rejection_Reason', Rejection_Reason FROM Vouchers WHERE NULLIF(LTRIM(RTRIM(Rejection_Reason)), '') IS NOT NULL;
GO

-- --------------------------------------------------------------------
-- STEP 2: Drop the columns
-- --------------------------------------------------------------------
ALTER TABLE Vouchers DROP COLUMN Deposit_Proof, Payer_Details, Rejection_Reason;
GO

-- --------------------------------------------------------------------
-- STEP 3: Check
-- --------------------------------------------------------------------
SELECT Voucher_ID, Column_Name, LEFT(Value, 60) AS Value FROM Vouchers_RetiredColumns;  -- expect 1 row (UASU-MMU-0047/26)
GO
