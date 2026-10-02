-- No USE statement on purpose: run this with the target database chosen
-- explicitly (sqlcmd -d UASU_Finance_Test, or the SSMS database picker),
-- so it can't land on the live database by accident. Apply it to
-- UASU_Finance_Test first; run it on UASU_Finance_Web only once the
-- feature has been signed off in dev, and take a backup before you do.

-- ====================================================================
-- Cash floats (imprest): cash drawn from the bank and handed to one
-- official to spend on a specific purpose - a repairs project, the
-- treasury's petty cash, a strike fund - who then accounts for it with
-- receipts.
--
-- How the money moves through the books
--   1. The bank withdrawal itself already moves the money from Bank to
--      Cash (BankWithdrawals, unchanged).
--   2. Issuing a float only records that part of that cash is now with
--      a custodian. It is still the union's cash, so the cash book does
--      not change.
--   3. The custodian hands in receipts; the treasury records each one
--      (CashFloatReceipts). Nothing hits the books yet.
--   4. The treasury retires receipts: the app raises one ordinary
--      Expense voucher per budget line, payee = the custodian, and the
--      receipts point at it (CashFloatReceipts.Voucher_ID). The voucher
--      goes through the normal Chairman + Chapter Secretary approval.
--   5. Once approved, the treasury settles it: an ordinary Cash payment,
--      linked to the float's withdrawal. That is the moment the spend
--      reaches the cash book, budget performance and certification - the
--      same path every other cash payment already takes.
--   6. Unspent cash the custodian gives back is recorded as a return
--      (CashFloatReturns). It stays in the cash bucket; it is just back
--      with the treasury instead of the custodian.
--   A rejected retirement voucher releases its receipts so they can be
--   corrected and retired again.
--
-- What this adds
--   - CashFloats, CashFloatReceipts, CashFloatReturns
--   - fn_CashFloatPosition(@AsOf): ONE definition of each float's
--     position as at a date, used by the float pages, the withdrawal
--     balance and Monthly Certification so they cannot disagree.
--   - Triggers enforcing, in the database and not just the app:
--       - receipts + returns never exceed what was issued
--       - nothing is added to a closed float
--       - a receipt on a live (not rejected) retirement voucher cannot be
--         edited or deleted
--       - a retirement voucher's receipts match it: same custodian as
--         payee, same budget line, and they add up to its amount
--       - a retirement voucher's amount, payee and budget line cannot be
--         edited away from its receipts
--       - a float can only be closed once all its cash is accounted for
--         and every receipt has been settled
--
-- Nothing existing is altered: no table, view or trigger from earlier
-- scripts changes. Each step is its own batch (GO); if a step errors,
-- stop and send the message rather than continuing.
-- ====================================================================

-- --------------------------------------------------------------------
-- STEP 1: Tables
-- --------------------------------------------------------------------
CREATE SEQUENCE seq_CashFloatID AS INT START WITH 1 INCREMENT BY 1;
GO

CREATE TABLE CashFloats (
    Float_ID NVARCHAR(20) NOT NULL PRIMARY KEY
        DEFAULT ('FLT-' + CONVERT(VARCHAR(4), DATEPART(YEAR, GETDATE())) + '-'
                 + RIGHT('0000' + CONVERT(VARCHAR(4), NEXT VALUE FOR seq_CashFloatID), 4)),
    Float_Type NVARCHAR(30) NOT NULL
        CONSTRAINT CK_CashFloats_Type CHECK (Float_Type IN ('Project', 'Petty Cash', 'Strike Fund', 'Other')),
    Purpose NVARCHAR(300) NOT NULL,
    Custodian_ID NVARCHAR(20) NOT NULL
        CONSTRAINT FK_CashFloats_Custodian FOREIGN KEY REFERENCES Ref_Officials(OfficialID),
    -- NULL when the float was handed out of cash the treasury already held
    -- rather than drawn for this purpose.
    Withdrawal_ID NVARCHAR(20) NULL
        CONSTRAINT FK_CashFloats_Withdrawal FOREIGN KEY REFERENCES BankWithdrawals(Withdrawal_ID),
    Issue_Date DATE NOT NULL,
    Amount DECIMAL(12,2) NOT NULL
        CONSTRAINT CK_CashFloats_Amount CHECK (Amount > 0),
    Issued_By NVARCHAR(20) NOT NULL
        CONSTRAINT FK_CashFloats_IssuedBy FOREIGN KEY REFERENCES Ref_Officials(OfficialID),
    Recorded_Date DATETIME NOT NULL DEFAULT GETDATE(),
    -- NULL while open.
    Closed_Date DATE NULL,
    Notes NVARCHAR(500) NULL
);
GO

CREATE INDEX IX_CashFloats_Withdrawal ON CashFloats (Withdrawal_ID);
GO

CREATE TABLE CashFloatReceipts (
    Receipt_Line_ID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Float_ID NVARCHAR(20) NOT NULL
        CONSTRAINT FK_CashFloatReceipts_Float FOREIGN KEY REFERENCES CashFloats(Float_ID),
    Receipt_Date DATE NOT NULL,
    Amount DECIMAL(12,2) NOT NULL
        CONSTRAINT CK_CashFloatReceipts_Amount CHECK (Amount > 0),
    Vendor NVARCHAR(150) NOT NULL,
    Receipt_No NVARCHAR(50) NULL,
    [Description] NVARCHAR(300) NOT NULL,
    Budget_Link NVARCHAR(20) NOT NULL
        CONSTRAINT FK_CashFloatReceipts_Budget FOREIGN KEY REFERENCES Ref_BudgetCodes(Budget_ID),
    -- The retirement voucher this receipt is on. NULL until retired; a
    -- rejected voucher leaves it pointing there until it is retired again.
    Voucher_ID NVARCHAR(20) NULL
        CONSTRAINT FK_CashFloatReceipts_Voucher FOREIGN KEY REFERENCES Vouchers(Voucher_ID),
    Recorded_By NVARCHAR(20) NOT NULL
        CONSTRAINT FK_CashFloatReceipts_RecordedBy FOREIGN KEY REFERENCES Ref_Officials(OfficialID),
    Recorded_Date DATETIME NOT NULL DEFAULT GETDATE(),
    -- Optional scan/photo of the receipt (JPEG, PNG or PDF, checked by
    -- file signature in the app before it is stored).
    Attachment_Data VARBINARY(MAX) NULL,
    Attachment_FileName NVARCHAR(255) NULL,
    Attachment_ContentType NVARCHAR(100) NULL
);
GO

CREATE INDEX IX_CashFloatReceipts_Float ON CashFloatReceipts (Float_ID);
CREATE INDEX IX_CashFloatReceipts_Voucher ON CashFloatReceipts (Voucher_ID);
GO

CREATE TABLE CashFloatReturns (
    Return_ID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Float_ID NVARCHAR(20) NOT NULL
        CONSTRAINT FK_CashFloatReturns_Float FOREIGN KEY REFERENCES CashFloats(Float_ID),
    Return_Date DATE NOT NULL,
    Amount DECIMAL(12,2) NOT NULL
        CONSTRAINT CK_CashFloatReturns_Amount CHECK (Amount > 0),
    Notes NVARCHAR(300) NULL,
    Recorded_By NVARCHAR(20) NOT NULL
        CONSTRAINT FK_CashFloatReturns_RecordedBy FOREIGN KEY REFERENCES Ref_Officials(OfficialID),
    Recorded_Date DATETIME NOT NULL DEFAULT GETDATE()
);
GO

CREATE INDEX IX_CashFloatReturns_Float ON CashFloatReturns (Float_ID);
GO

-- --------------------------------------------------------------------
-- STEP 2: fn_CashFloatPosition - each float's position as at a date
--
--   Spent           receipts dated on or before @AsOf
--   Settled         receipts whose retirement voucher was paid on or
--                   before @AsOf (only these have reached the books)
--   AwaitingVoucher receipts with no live retirement voucher (never
--                   retired, or retired on a voucher that was rejected)
--   Returned        cash handed back on or before @AsOf
--   CashWithCustodian  Issued - Spent - Returned: what the custodian
--                   should physically be holding
--   BookOutstanding Issued - Settled - Returned: how much of the book
--                   Cash balance is out with this custodian (physical
--                   cash plus receipts not yet settled)
--
-- Floats issued after @AsOf are left out entirely.
-- --------------------------------------------------------------------
CREATE FUNCTION fn_CashFloatPosition (@AsOf DATE)
RETURNS TABLE
AS
RETURN
    SELECT
        f.Float_ID, f.Float_Type, f.Purpose, f.Custodian_ID, f.Withdrawal_ID,
        f.Issue_Date, f.Closed_Date,
        f.Amount AS Issued,
        ISNULL(r.Spent, 0) AS Spent,
        ISNULL(r.Settled, 0) AS Settled,
        ISNULL(r.AwaitingVoucher, 0) AS AwaitingVoucher,
        ISNULL(rt.Returned, 0) AS Returned,
        f.Amount - ISNULL(r.Spent, 0) - ISNULL(rt.Returned, 0) AS CashWithCustodian,
        f.Amount - ISNULL(r.Settled, 0) - ISNULL(rt.Returned, 0) AS BookOutstanding
    FROM CashFloats f
    OUTER APPLY (
        -- Flags worked out per receipt first: SQL Server can't SUM over
        -- an expression that contains a subquery.
        SELECT
            SUM(x.Amount) AS Spent,
            SUM(CASE WHEN x.IsSettled = 1 THEN x.Amount ELSE 0 END) AS Settled,
            SUM(CASE WHEN x.IsAwaitingVoucher = 1 THEN x.Amount ELSE 0 END) AS AwaitingVoucher
        FROM (
            SELECT cr.Amount,
                   CASE WHEN EXISTS (
                           SELECT 1 FROM PaymentAllocations pa
                           INNER JOIN Payments p ON p.Payment_ID = pa.Payment_ID
                           WHERE pa.Voucher_ID = cr.Voucher_ID AND p.Payment_Date <= @AsOf)
                        THEN 1 ELSE 0 END AS IsSettled,
                   CASE WHEN cr.Voucher_ID IS NULL OR EXISTS (
                           SELECT 1 FROM Voucher_Approvals va
                           WHERE va.Voucher_ID = cr.Voucher_ID AND va.Approval_Status = 'Rejected')
                        THEN 1 ELSE 0 END AS IsAwaitingVoucher
            FROM CashFloatReceipts cr
            WHERE cr.Float_ID = f.Float_ID AND cr.Receipt_Date <= @AsOf
        ) x
    ) r
    OUTER APPLY (
        SELECT SUM(x.Amount) AS Returned
        FROM CashFloatReturns x
        WHERE x.Float_ID = f.Float_ID AND x.Return_Date <= @AsOf
    ) rt
    WHERE f.Issue_Date <= @AsOf;
GO

-- --------------------------------------------------------------------
-- STEP 3: Receipts - balance, closed float, retirement voucher match,
-- and no change to a receipt on a live retirement voucher
-- --------------------------------------------------------------------
CREATE TRIGGER trg_CashFloatReceipts_Validate
ON CashFloatReceipts
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;

    -- A receipt that is on a live retirement voucher is part of what the
    -- approvers signed off: only its Voucher_ID may move (to a new voucher
    -- after a rejection is handled by the release rule below), nothing else.
    IF EXISTS (
        SELECT 1
        FROM deleted d
        LEFT JOIN inserted i ON i.Receipt_Line_ID = d.Receipt_Line_ID
        WHERE d.Voucher_ID IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM Voucher_Approvals va
                          WHERE va.Voucher_ID = d.Voucher_ID AND va.Approval_Status = 'Rejected')
          AND (i.Receipt_Line_ID IS NULL
               OR i.Float_ID <> d.Float_ID OR i.Amount <> d.Amount OR i.Budget_Link <> d.Budget_Link
               OR i.Receipt_Date <> d.Receipt_Date
               OR ISNULL(i.Voucher_ID, '') <> d.Voucher_ID)
    )
    BEGIN
        RAISERROR('This receipt is on a retirement voucher that has not been rejected, so it cannot be changed or removed.',16,1);
        ROLLBACK TRANSACTION;
        RETURN;
    END

    -- New receipts can't go on a closed float.
    IF EXISTS (
        SELECT 1 FROM inserted i
        INNER JOIN CashFloats f ON f.Float_ID = i.Float_ID
        WHERE f.Closed_Date IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM deleted d WHERE d.Receipt_Line_ID = i.Receipt_Line_ID)
    )
    BEGIN
        RAISERROR('This float is closed. Receipts cannot be added to it.',16,1);
        ROLLBACK TRANSACTION;
        RETURN;
    END

    -- Receipts plus returns can never exceed what was issued.
    IF EXISTS (
        SELECT 1
        FROM CashFloats f
        WHERE f.Float_ID IN (SELECT Float_ID FROM inserted)
          AND f.Amount < ISNULL((SELECT SUM(Amount) FROM CashFloatReceipts r WHERE r.Float_ID = f.Float_ID), 0)
                       + ISNULL((SELECT SUM(Amount) FROM CashFloatReturns x WHERE x.Float_ID = f.Float_ID), 0)
    )
    BEGIN
        RAISERROR('Receipts and returns on this float would add up to more than the cash issued.',16,1);
        ROLLBACK TRANSACTION;
        RETURN;
    END

    -- Every voucher a receipt now points at must be a matching retirement
    -- voucher: Expense, paid to the float's custodian, on the receipt's
    -- budget line, and exactly the sum of its receipts.
    IF EXISTS (
        SELECT 1
        FROM Vouchers v
        WHERE v.Voucher_ID IN (SELECT Voucher_ID FROM inserted WHERE Voucher_ID IS NOT NULL)
          AND (
                v.Transaction_Type <> 'Expense'
             OR v.Amount <> (SELECT SUM(r.Amount) FROM CashFloatReceipts r WHERE r.Voucher_ID = v.Voucher_ID)
             OR EXISTS (
                    SELECT 1
                    FROM CashFloatReceipts r
                    INNER JOIN CashFloats f ON f.Float_ID = r.Float_ID
                    WHERE r.Voucher_ID = v.Voucher_ID
                      AND (v.Official_Link IS NULL OR v.Official_Link <> f.Custodian_ID
                           OR v.Budget_Link IS NULL OR v.Budget_Link <> r.Budget_Link))
              )
    )
    BEGIN
        RAISERROR('A retirement voucher must be an Expense voucher paid to the float''s custodian, on the receipts'' budget line, for exactly the total of its receipts.',16,1);
        ROLLBACK TRANSACTION;
        RETURN;
    END
END;
GO

-- --------------------------------------------------------------------
-- STEP 4: Returns - balance and closed float
-- --------------------------------------------------------------------
CREATE TRIGGER trg_CashFloatReturns_Validate
ON CashFloatReturns
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (
        SELECT 1 FROM inserted i
        INNER JOIN CashFloats f ON f.Float_ID = i.Float_ID
        WHERE f.Closed_Date IS NOT NULL
    )
    BEGIN
        RAISERROR('This float is closed. Returns cannot be recorded against it.',16,1);
        ROLLBACK TRANSACTION;
        RETURN;
    END

    IF EXISTS (
        SELECT 1
        FROM CashFloats f
        WHERE f.Float_ID IN (SELECT Float_ID FROM inserted)
          AND f.Amount < ISNULL((SELECT SUM(Amount) FROM CashFloatReceipts r WHERE r.Float_ID = f.Float_ID), 0)
                       + ISNULL((SELECT SUM(Amount) FROM CashFloatReturns x WHERE x.Float_ID = f.Float_ID), 0)
    )
    BEGIN
        RAISERROR('Receipts and returns on this float would add up to more than the cash issued.',16,1);
        ROLLBACK TRANSACTION;
        RETURN;
    END
END;
GO

-- --------------------------------------------------------------------
-- STEP 5: Floats - amount can't drop below what's accounted for, and a
-- float only closes once fully accounted for and settled
-- --------------------------------------------------------------------
CREATE TRIGGER trg_CashFloats_Validate
ON CashFloats
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (
        SELECT 1
        FROM inserted f
        WHERE f.Amount < ISNULL((SELECT SUM(Amount) FROM CashFloatReceipts r WHERE r.Float_ID = f.Float_ID), 0)
                       + ISNULL((SELECT SUM(Amount) FROM CashFloatReturns x WHERE x.Float_ID = f.Float_ID), 0)
    )
    BEGIN
        RAISERROR('A float''s amount cannot be less than its receipts and returns.',16,1);
        ROLLBACK TRANSACTION;
        RETURN;
    END

    IF EXISTS (
        SELECT 1
        FROM inserted f
        INNER JOIN fn_CashFloatPosition('9999-12-31') p ON p.Float_ID = f.Float_ID
        WHERE f.Closed_Date IS NOT NULL
          AND (p.CashWithCustodian <> 0 OR p.BookOutstanding <> 0)
    )
    BEGIN
        RAISERROR('A float can only be closed once all its cash is accounted for (spent or returned) and every receipt has been settled.',16,1);
        ROLLBACK TRANSACTION;
        RETURN;
    END
END;
GO

-- --------------------------------------------------------------------
-- STEP 6: Retirement vouchers can't be edited away from their receipts
-- (same idea as trg_Vouchers_LockDebtVoucher in SQL/031)
-- --------------------------------------------------------------------
CREATE TRIGGER trg_Vouchers_LockFloatVoucher
ON Vouchers
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT (UPDATE(Amount) OR UPDATE(Official_Link) OR UPDATE(Budget_Link) OR UPDATE(Transaction_Type))
        RETURN;

    IF EXISTS (
        SELECT 1
        FROM inserted i
        WHERE EXISTS (SELECT 1 FROM CashFloatReceipts r WHERE r.Voucher_ID = i.Voucher_ID)
          AND (
                i.Transaction_Type <> 'Expense'
             OR i.Amount <> (SELECT SUM(r.Amount) FROM CashFloatReceipts r WHERE r.Voucher_ID = i.Voucher_ID)
             OR EXISTS (
                    SELECT 1
                    FROM CashFloatReceipts r
                    INNER JOIN CashFloats f ON f.Float_ID = r.Float_ID
                    WHERE r.Voucher_ID = i.Voucher_ID
                      AND (i.Official_Link IS NULL OR i.Official_Link <> f.Custodian_ID
                           OR i.Budget_Link IS NULL OR i.Budget_Link <> r.Budget_Link))
              )
    )
    BEGIN
        RAISERROR('This voucher retires cash float receipts, so its amount, payee and budget line cannot be changed. Reject it and retire the receipts again from the float.',16,1);
        ROLLBACK TRANSACTION;
        RETURN;
    END
END;
GO
