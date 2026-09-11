-- ====================================================================
-- Migrate historical data from UASU_Finance_01 (old) into
-- UASU_Finance_Web (the database the app actually uses).
--
-- ASSUMES both databases live on the same SQL Server instance, so
-- three-part names (UASU_Finance_01.dbo.TableName) work directly with
-- no linked server needed. If they're on different servers, this
-- script needs a linked server first - ask before adapting it that way.
--
-- BEFORE RUNNING: take a full backup of UASU_Finance_Web. This script
-- writes real data into a database your live app depends on.
--
-- Run each numbered section in order. Sections 1-2 are READ-ONLY and
-- safe to run and re-run freely - review their output before touching
-- section 3. Section 3 is the only part that writes data, and it's
-- wrapped in a single transaction so it either fully succeeds or
-- fully rolls back.
-- ====================================================================


-- ====================================================================
-- SECTION 1 (read-only): row counts on both sides, so you know what
-- you're starting from and can sanity-check the result afterward.
-- ====================================================================
SELECT 'Ref_BudgetCodes' AS TableName,
       (SELECT COUNT(*) FROM UASU_Finance_01.dbo.Ref_BudgetCodes) AS OldCount,
       (SELECT COUNT(*) FROM UASU_Finance_Web.dbo.Ref_BudgetCodes) AS WebCount
UNION ALL
SELECT 'Ref_Officials',
       (SELECT COUNT(*) FROM UASU_Finance_01.dbo.Ref_Officials),
       (SELECT COUNT(*) FROM UASU_Finance_Web.dbo.Ref_Officials)
UNION ALL
SELECT 'Ref_Suppliers',
       (SELECT COUNT(*) FROM UASU_Finance_01.dbo.Ref_Suppliers),
       (SELECT COUNT(*) FROM UASU_Finance_Web.dbo.Ref_Suppliers)
UNION ALL
SELECT 'Debt_Register',
       (SELECT COUNT(*) FROM UASU_Finance_01.dbo.Debt_Register),
       (SELECT COUNT(*) FROM UASU_Finance_Web.dbo.Debt_Register)
UNION ALL
SELECT 'Vouchers',
       (SELECT COUNT(*) FROM UASU_Finance_01.dbo.Vouchers),
       (SELECT COUNT(*) FROM UASU_Finance_Web.dbo.Vouchers)
UNION ALL
SELECT 'Payments',
       (SELECT COUNT(*) FROM UASU_Finance_01.dbo.Payments),
       (SELECT COUNT(*) FROM UASU_Finance_Web.dbo.Payments)
UNION ALL
SELECT 'Proposed_Budget',
       (SELECT COUNT(*) FROM UASU_Finance_01.dbo.Proposed_Budget),
       (SELECT COUNT(*) FROM UASU_Finance_Web.dbo.Proposed_Budget)
UNION ALL
SELECT 'BankWithdrawals',
       (SELECT COUNT(*) FROM UASU_Finance_01.dbo.BankWithdrawals),
       (SELECT COUNT(*) FROM UASU_Finance_Web.dbo.BankWithdrawals)
UNION ALL
SELECT 'WithdrawalPayments',
       (SELECT COUNT(*) FROM UASU_Finance_01.dbo.WithdrawalPayments),
       (SELECT COUNT(*) FROM UASU_Finance_Web.dbo.WithdrawalPayments)
UNION ALL
SELECT 'PaymentCharges',
       (SELECT COUNT(*) FROM UASU_Finance_01.dbo.PaymentCharges),
       (SELECT COUNT(*) FROM UASU_Finance_Web.dbo.PaymentCharges)
UNION ALL
SELECT 'PaymentAllocations',
       (SELECT COUNT(*) FROM UASU_Finance_01.dbo.PaymentAllocations),
       (SELECT COUNT(*) FROM UASU_Finance_Web.dbo.PaymentAllocations);


-- ====================================================================
-- SECTION 2 (read-only): collision check on the tables most likely to
-- have matching IDs between the two databases, since both generate
-- IDs from their own independent sequence starting at 1. A row here
-- means the SAME ID exists on both sides - review whether it's really
-- the same real-world record (safe to skip) or two different things
-- that happen to share an ID (would need manual handling before
-- Section 3, since Section 3 silently skips anything already present
-- in the target by ID).
-- ====================================================================
SELECT 'Ref_Officials' AS TableName, o1.OfficialID, o1.FullName AS OldName, o2.FullName AS WebName
FROM UASU_Finance_01.dbo.Ref_Officials o1
INNER JOIN UASU_Finance_Web.dbo.Ref_Officials o2 ON o1.OfficialID = o2.OfficialID
WHERE o1.FullName <> o2.FullName;

SELECT 'Ref_BudgetCodes' AS TableName, b1.Budget_ID, b1.Category_Name AS OldName, b2.Category_Name AS WebName
FROM UASU_Finance_01.dbo.Ref_BudgetCodes b1
INNER JOIN UASU_Finance_Web.dbo.Ref_BudgetCodes b2 ON b1.Budget_ID = b2.Budget_ID
WHERE b1.Category_Name <> b2.Category_Name;

SELECT 'Vouchers' AS TableName, v1.Voucher_ID, v1.Amount AS OldAmount, v2.Amount AS WebAmount
FROM UASU_Finance_01.dbo.Vouchers v1
INNER JOIN UASU_Finance_Web.dbo.Vouchers v2 ON v1.Voucher_ID = v2.Voucher_ID
WHERE v1.Amount <> v2.Amount OR v1.VoucherDate <> v2.VoucherDate;

SELECT 'Payments' AS TableName, p1.Payment_ID, p1.Amount_Paid AS OldAmount, p2.Amount_Paid AS WebAmount
FROM UASU_Finance_01.dbo.Payments p1
INNER JOIN UASU_Finance_Web.dbo.Payments p2 ON p1.Payment_ID = p2.Payment_ID
WHERE p1.Amount_Paid <> p2.Amount_Paid OR p1.Payment_Date <> p2.Payment_Date;

-- If any of the four queries above return rows, STOP and review them
-- manually before proceeding - it means the same ID represents two
-- different real records in the old and new databases, which Section 3
-- cannot safely resolve on its own.


-- ====================================================================
-- SECTION 3 (writes data): the actual copy, in dependency order.
-- Every INSERT is guarded by "ID not already present in the target",
-- so this section is safe to re-run - already-migrated rows are
-- skipped, not duplicated. Wrapped in one transaction: if anything
-- fails partway through, everything rolls back together rather than
-- leaving the target half-migrated.
-- ====================================================================
BEGIN TRANSACTION MigrateLegacyData;

BEGIN TRY

    -- 3.1 Ref_BudgetCodes
    INSERT INTO UASU_Finance_Web.dbo.Ref_BudgetCodes (Budget_ID, Category_Name, Category_Type)
    SELECT s.Budget_ID, s.Category_Name, s.Category_Type
    FROM UASU_Finance_01.dbo.Ref_BudgetCodes s
    WHERE NOT EXISTS (
        SELECT 1 FROM UASU_Finance_Web.dbo.Ref_BudgetCodes t WHERE t.Budget_ID = s.Budget_ID
    );

    -- 3.2 Ref_Officials
    INSERT INTO UASU_Finance_Web.dbo.Ref_Officials (OfficialID, FullName, Role, IsCurrent, Email, Phone_No)
    SELECT s.OfficialID, s.FullName, s.Role, s.IsCurrent, s.Email, s.Phone_No
    FROM UASU_Finance_01.dbo.Ref_Officials s
    WHERE NOT EXISTS (
        SELECT 1 FROM UASU_Finance_Web.dbo.Ref_Officials t WHERE t.OfficialID = s.OfficialID
    );

    -- 3.3 Ref_Suppliers
    INSERT INTO UASU_Finance_Web.dbo.Ref_Suppliers (Supplier_ID, Business_Name, Service_Category, Tax_PIN, Contact_Phone, Email)
    SELECT s.Supplier_ID, s.Business_Name, s.Service_Category, s.Tax_PIN, s.Contact_Phone, s.Email
    FROM UASU_Finance_01.dbo.Ref_Suppliers s
    WHERE NOT EXISTS (
        SELECT 1 FROM UASU_Finance_Web.dbo.Ref_Suppliers t WHERE t.Supplier_ID = s.Supplier_ID
    );

    -- 3.4 Debt_Register (depends on Ref_Officials, Ref_Suppliers above)
    INSERT INTO UASU_Finance_Web.dbo.Debt_Register
        (Debt_ID, Creditor_Type, Official_link, Supplier_link, Date_Incurred, Invoice_No, Description, Total_Owed, Is_Settled)
    SELECT s.Debt_ID, s.Creditor_Type, s.Official_link, s.Supplier_link, s.Date_Incurred, s.Invoice_No, s.Description, s.Total_Owed, s.Is_Settled
    FROM UASU_Finance_01.dbo.Debt_Register s
    WHERE NOT EXISTS (
        SELECT 1 FROM UASU_Finance_Web.dbo.Debt_Register t WHERE t.Debt_ID = s.Debt_ID
    );

    -- 3.5 Vouchers - Is_Legacy is hardcoded to 1 here, not backfilled
    -- afterward: every voucher coming from UASU_Finance_01 predates
    -- this approval workflow by definition, so there's no ambiguity
    -- to resolve later the way SQL/002's heuristic backfill had to.
    INSERT INTO UASU_Finance_Web.dbo.Vouchers
        (Voucher_ID, VoucherDate, Transaction_Type, Budget_Link, Official_Link, Supplier_Link, Amount,
         Current_Status, [Description], Debt_link, Manual_Payee_Name, Payee_Category,
         Deposit_Proof, Payer_Details, Rejection_Reason, Is_Legacy)
    SELECT s.Voucher_ID, s.VoucherDate, s.Transaction_Type, s.Budget_Link, s.Official_Link, s.Supplier_Link, s.Amount,
           s.Current_Status, s.[Description], s.Debt_link, s.Manual_Payee_Name, s.Payee_Category,
           s.Deposit_Proof, s.Payer_Details, s.Rejection_Reason, 1
    FROM UASU_Finance_01.dbo.Vouchers s
    WHERE NOT EXISTS (
        SELECT 1 FROM UASU_Finance_Web.dbo.Vouchers t WHERE t.Voucher_ID = s.Voucher_ID
    );

    -- 3.6 Payments
    INSERT INTO UASU_Finance_Web.dbo.Payments (Payment_ID, Payment_Date, Payment_Mode, [Description], Amount_Paid, Reference_No, Bank_Account)
    SELECT s.Payment_ID, s.Payment_Date, s.Payment_Mode, s.[Description], s.Amount_Paid, s.Reference_No, s.Bank_Account
    FROM UASU_Finance_01.dbo.Payments s
    WHERE NOT EXISTS (
        SELECT 1 FROM UASU_Finance_Web.dbo.Payments t WHERE t.Payment_ID = s.Payment_ID
    );

    -- 3.7 Proposed_Budget - Proposal_ID is an IDENTITY column with no
    -- child table referencing it, so new IDs are assigned automatically
    -- rather than trying to preserve the old ones.
    INSERT INTO UASU_Finance_Web.dbo.Proposed_Budget (Budget_Link, Fiscal_Year, Proposed_Amount, Notes, Date_Added)
    SELECT s.Budget_Link, s.Fiscal_Year, s.Proposed_Amount, s.Notes, s.Date_Added
    FROM UASU_Finance_01.dbo.Proposed_Budget s
    WHERE NOT EXISTS (
        SELECT 1 FROM UASU_Finance_Web.dbo.Proposed_Budget t
        WHERE t.Budget_Link = s.Budget_Link AND t.Fiscal_Year = s.Fiscal_Year AND t.Proposed_Amount = s.Proposed_Amount
    );

    -- 3.8 BankWithdrawals
    INSERT INTO UASU_Finance_Web.dbo.BankWithdrawals (Withdrawal_ID, Withdrawal_Date, Amount, Reference_No, Bank_Account, Notes)
    SELECT s.Withdrawal_ID, s.Withdrawal_Date, s.Amount, s.Reference_No, s.Bank_Account, s.Notes
    FROM UASU_Finance_01.dbo.BankWithdrawals s
    WHERE NOT EXISTS (
        SELECT 1 FROM UASU_Finance_Web.dbo.BankWithdrawals t WHERE t.Withdrawal_ID = s.Withdrawal_ID
    );

    -- 3.9 WithdrawalPayments (depends on BankWithdrawals, Payments above)
    INSERT INTO UASU_Finance_Web.dbo.WithdrawalPayments (Withdrawal_ID, Payment_ID, Allocated_Amount)
    SELECT s.Withdrawal_ID, s.Payment_ID, s.Allocated_Amount
    FROM UASU_Finance_01.dbo.WithdrawalPayments s
    WHERE NOT EXISTS (
        SELECT 1 FROM UASU_Finance_Web.dbo.WithdrawalPayments t
        WHERE t.Withdrawal_ID = s.Withdrawal_ID AND t.Payment_ID = s.Payment_ID
    );

    -- 3.10 PaymentCharges (depends on WithdrawalPayments above, since
    -- trg_PaymentCharges_ValidateWithdrawal checks against it)
    INSERT INTO UASU_Finance_Web.dbo.PaymentCharges (Charge_ID, Payment_ID, Withdrawal_ID, Charge_Type, Charge_Amount, Notes)
    SELECT s.Charge_ID, s.Payment_ID, s.Withdrawal_ID, s.Charge_Type, s.Charge_Amount, s.Notes
    FROM UASU_Finance_01.dbo.PaymentCharges s
    WHERE NOT EXISTS (
        SELECT 1 FROM UASU_Finance_Web.dbo.PaymentCharges t WHERE t.Charge_ID = s.Charge_ID
    );

    -- 3.11 PaymentAllocations - LAST, because trg_PaymentAllocations_
    -- RequireApproval in the target fires on every row here, and only
    -- allows it through because Is_Legacy was already set to 1 on the
    -- voucher in step 3.5, above.
    INSERT INTO UASU_Finance_Web.dbo.PaymentAllocations (Payment_ID, Voucher_ID, Allocated_Amount)
    SELECT s.Payment_ID, s.Voucher_ID, s.Allocated_Amount
    FROM UASU_Finance_01.dbo.PaymentAllocations s
    WHERE NOT EXISTS (
        SELECT 1 FROM UASU_Finance_Web.dbo.PaymentAllocations t
        WHERE t.Payment_ID = s.Payment_ID AND t.Voucher_ID = s.Voucher_ID
    );

    COMMIT TRANSACTION MigrateLegacyData;
    PRINT 'Migration committed successfully.';

END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION MigrateLegacyData;
    PRINT 'Migration FAILED and was rolled back. No data was changed. Error follows:';
    THROW;
END CATCH;


-- ====================================================================
-- SECTION 4: reseed every sequence in UASU_Finance_Web past the
-- highest ID now present, so the app's own NEXT VALUE FOR calls can
-- never generate an ID that collides with something just migrated.
-- Safe to run even if nothing was migrated (falls back to 1).
-- ====================================================================
USE UASU_Finance_Web;
GO

DECLARE @next INT, @sql NVARCHAR(200);

SELECT @next = ISNULL(MAX(CAST(SUBSTRING(Budget_ID, 5, 3) AS INT)), 0) + 1 FROM Ref_BudgetCodes;
SET @sql = 'ALTER SEQUENCE BudgetSeq RESTART WITH ' + CAST(@next AS NVARCHAR(10));
EXEC(@sql);

SELECT @next = ISNULL(MAX(CAST(SUBSTRING(OfficialID, 4, 3) AS INT)), 0) + 1 FROM Ref_Officials;
SET @sql = 'ALTER SEQUENCE OfficialSeq RESTART WITH ' + CAST(@next AS NVARCHAR(10));
EXEC(@sql);

SELECT @next = ISNULL(MAX(CAST(SUBSTRING(Supplier_ID, 5, 3) AS INT)), 0) + 1 FROM Ref_Suppliers;
SET @sql = 'ALTER SEQUENCE SupplierSeq RESTART WITH ' + CAST(@next AS NVARCHAR(10));
EXEC(@sql);

SELECT @next = ISNULL(MAX(CAST(SUBSTRING(Debt_ID, 5, 3) AS INT)), 0) + 1 FROM Debt_Register;
SET @sql = 'ALTER SEQUENCE DebtSeq RESTART WITH ' + CAST(@next AS NVARCHAR(10));
EXEC(@sql);

-- Voucher_ID looks like 'UASU-MMU-0004/25' - the counter is the 4
-- digits between the second '-' and the '/', independent of the year.
SELECT @next = ISNULL(MAX(CAST(SUBSTRING(Voucher_ID, 10, 4) AS INT)), 0) + 1 FROM Vouchers;
SET @sql = 'ALTER SEQUENCE VoucherSeq RESTART WITH ' + CAST(@next AS NVARCHAR(10));
EXEC(@sql);

-- Payment_ID looks like 'PAY-2025-0004' - counter is the last 4 digits.
SELECT @next = ISNULL(MAX(CAST(RIGHT(Payment_ID, 4) AS INT)), 0) + 1 FROM Payments;
SET @sql = 'ALTER SEQUENCE seq_PaymentID RESTART WITH ' + CAST(@next AS NVARCHAR(10));
EXEC(@sql);

SELECT @next = ISNULL(MAX(CAST(SUBSTRING(Withdrawal_ID, 5, 3) AS INT)), 0) + 1 FROM BankWithdrawals;
SET @sql = 'ALTER SEQUENCE BankWithdrawalSeq RESTART WITH ' + CAST(@next AS NVARCHAR(10));
EXEC(@sql);

SELECT @next = ISNULL(MAX(CAST(SUBSTRING(Charge_ID, 5, 3) AS INT)), 0) + 1 FROM PaymentCharges;
SET @sql = 'ALTER SEQUENCE PayChargesSeq RESTART WITH ' + CAST(@next AS NVARCHAR(10));
EXEC(@sql);

PRINT 'Sequences reseeded past the highest migrated ID in each table.';


-- ====================================================================
-- SECTION 5 (read-only): re-run the same counts from Section 1 to
-- confirm the target grew by roughly the expected amount.
-- ====================================================================
SELECT 'Vouchers' AS TableName, COUNT(*) AS WebCountAfterMigration, SUM(CASE WHEN Is_Legacy = 1 THEN 1 ELSE 0 END) AS LegacyCount
FROM UASU_Finance_Web.dbo.Vouchers
UNION ALL
SELECT 'Payments', COUNT(*), NULL FROM UASU_Finance_Web.dbo.Payments
UNION ALL
SELECT 'PaymentAllocations', COUNT(*), NULL FROM UASU_Finance_Web.dbo.PaymentAllocations;
