USE UASU_Finance_Web;  -- adjust if your database name differs
GO

-- ====================================================================
-- Activity decisions now create a DEBT owed to the official (one per
-- Pay line) instead of a voucher; the treasury later raises vouchers
-- from those debts (Pay Debts page), and one voucher may pay SEVERAL
-- debts as long as they are all owed to the same official.
--
-- What this changes
--   1. VoucherDebts: links a voucher to each debt it pays (many debts to
--      one voucher). A voucher paying exactly one debt ALSO keeps
--      Vouchers.Debt_Link set, so the existing single-debt flow, screens
--      and dashboards behave exactly as before.
--   2. fn_DebtAllocations: ONE definition of "how much of each debt has
--      been paid", covering both ways a voucher can be linked to a debt.
--      DebtSummary, trg_PaymentStatusUpdate and the Balance Sheet's
--      Accounts Payable all use it, so they cannot disagree.
--   3. DebtSummary and trg_PaymentStatusUpdate are re-created to use it.
--      Their existing behaviour for single-debt vouchers is unchanged.
--   4. ActivityParticipants gets Debt_ID; a Pay line now carries a debt
--      (and no budget item - that is chosen when the voucher is raised).
--      Lines already decided under SQL/030 (which carry a voucher) stay
--      valid.
--   5. Rules enforced in the database, not just the app:
--        - every debt on a voucher is owed to that voucher's payee
--        - the debts on a voucher add up to the voucher's amount
--        - a debt cannot be in two live (not rejected) vouchers at once
--        - a voucher's amount/payee cannot be edited away from its debts
--
-- Not changed: the UnionFinanceDashboard*/Trends/AnnualReport and
-- YearlyPaymentsByOfficialsYTD views. The app does not read them; for a
-- voucher paying several debts they would show it as an ordinary voucher
-- without debt attribution.
--
-- Depends on SQL/029 and SQL/030.
--
-- RUN AGAINST A COPY OF THE DATABASE FIRST, and take a backup before
-- running on the live database: this replaces a trigger and a view that
-- the payment workflow depends on. Each step is its own batch (GO); if a
-- step errors, stop and send the message rather than continuing.
-- ====================================================================

-- --------------------------------------------------------------------
-- STEP 1: VoucherDebts
-- --------------------------------------------------------------------
CREATE TABLE VoucherDebts (
    Voucher_ID NVARCHAR(20) NOT NULL
        CONSTRAINT FK_VoucherDebts_Voucher FOREIGN KEY REFERENCES Vouchers(Voucher_ID),
    Debt_ID NVARCHAR(20) NOT NULL
        CONSTRAINT FK_VoucherDebts_Debt FOREIGN KEY REFERENCES Debt_Register(Debt_ID),
    -- The part of the voucher that pays this debt: always the debt's full amount.
    Amount DECIMAL(18,2) NOT NULL
        CONSTRAINT CK_VoucherDebts_Amount CHECK (Amount > 0),
    CONSTRAINT PK_VoucherDebts PRIMARY KEY (Voucher_ID, Debt_ID)
);
GO

CREATE INDEX IX_VoucherDebts_Debt ON VoucherDebts (Debt_ID);
GO

-- --------------------------------------------------------------------
-- STEP 2: fn_DebtAllocations - how much of each debt has been paid
-- --------------------------------------------------------------------
CREATE OR ALTER FUNCTION dbo.fn_DebtAllocations (@AsOfDate DATE)
RETURNS TABLE
AS
RETURN
    SELECT x.Debt_ID, SUM(x.Allocated) AS Allocated
    FROM (
        -- A voucher linked to one debt through Vouchers.Debt_Link, as always
        -- (a debt may have several such vouchers, e.g. instalments). Vouchers
        -- that also have VoucherDebts rows are counted in the branch below.
        SELECT v.Debt_Link AS Debt_ID, pa.Allocated_Amount AS Allocated
        FROM Vouchers v
        INNER JOIN PaymentAllocations pa ON pa.Voucher_ID = v.Voucher_ID
        INNER JOIN Payments p ON p.Payment_ID = pa.Payment_ID
        WHERE v.Debt_Link IS NOT NULL
          AND p.Payment_Date <= @AsOfDate
          AND NOT EXISTS (SELECT 1 FROM VoucherDebts vd WHERE vd.Voucher_ID = v.Voucher_ID)

        UNION ALL

        -- A voucher paying one or more debts through VoucherDebts: once the
        -- voucher is paid in full every debt on it is paid in full; before
        -- that each debt has its proportional share of what has been paid.
        SELECT vd.Debt_ID,
               CASE WHEN t.Paid >= v.Amount THEN vd.Amount
                    ELSE ROUND(t.Paid * vd.Amount / v.Amount, 2) END
        FROM VoucherDebts vd
        INNER JOIN Vouchers v ON v.Voucher_ID = vd.Voucher_ID
        CROSS APPLY (
            SELECT SUM(pa.Allocated_Amount) AS Paid
            FROM PaymentAllocations pa
            INNER JOIN Payments p ON p.Payment_ID = pa.Payment_ID
            WHERE pa.Voucher_ID = v.Voucher_ID AND p.Payment_Date <= @AsOfDate
        ) t
        WHERE t.Paid IS NOT NULL
    ) x
    GROUP BY x.Debt_ID;
GO

-- --------------------------------------------------------------------
-- STEP 3: DebtSummary - same columns as before, now fed by the function
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
    END AS SettlementStatus
FROM Debt_Register d
LEFT JOIN dbo.fn_DebtAllocations('9999-12-31') a ON a.Debt_ID = d.Debt_ID;
GO

-- --------------------------------------------------------------------
-- STEP 4: trg_PaymentStatusUpdate - voucher logic unchanged; the debt
-- logic now covers every debt a paid voucher is linked to.
-- --------------------------------------------------------------------
CREATE OR ALTER TRIGGER [dbo].[trg_PaymentStatusUpdate] ON PaymentAllocations AFTER INSERT AS
BEGIN
    DECLARE @VoucherID NVARCHAR(20);
    DECLARE @DebtID NVARCHAR(20);
    DECLARE @TotalAllocated DECIMAL(18,2);
    DECLARE @VoucherAmount DECIMAL(18,2);
    DECLARE @D NVARCHAR(20);
    DECLARE @DebtTotalAllocated DECIMAL(18,2);
    DECLARE @DebtTotalOwed DECIMAL(18,2);

    DECLARE cur CURSOR LOCAL FAST_FORWARD FOR
    SELECT pa.Voucher_ID, v.Debt_Link
    FROM inserted pa
    INNER JOIN Vouchers v ON pa.Voucher_ID = v.Voucher_ID;

    OPEN cur;
    FETCH NEXT FROM cur INTO @VoucherID, @DebtID;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        -- Calculate total allocations for this voucher
        SELECT @TotalAllocated = ISNULL(SUM(Allocated_Amount),0)
        FROM PaymentAllocations
        WHERE Voucher_ID = @VoucherID;

        -- Get voucher amount
        SELECT @VoucherAmount = Amount
        FROM Vouchers
        WHERE Voucher_ID = @VoucherID;

        -- Prevent overpayment
        IF @TotalAllocated > @VoucherAmount
        BEGIN
            RAISERROR('Payment exceeds voucher amount. Transaction cancelled.',16,1);
            ROLLBACK TRANSACTION;
            CLOSE cur; DEALLOCATE cur;
            RETURN;
        END

        -- Update voucher status (applies to both Income and Expense)
        IF @TotalAllocated = 0
            UPDATE Vouchers SET Current_Status = 'Pending' WHERE Voucher_ID = @VoucherID;
        ELSE IF @TotalAllocated < @VoucherAmount
            UPDATE Vouchers SET Current_Status = 'Partially Paid' WHERE Voucher_ID = @VoucherID;
        ELSE IF @TotalAllocated = @VoucherAmount
            UPDATE Vouchers SET Current_Status = 'Paid' WHERE Voucher_ID = @VoucherID;

        -- Debt-level logic: the voucher's own Debt_Link and/or every debt
        -- listed for it in VoucherDebts.
        DECLARE dcur CURSOR LOCAL FAST_FORWARD FOR
            SELECT @DebtID WHERE @DebtID IS NOT NULL
            UNION
            SELECT vd.Debt_ID FROM VoucherDebts vd WHERE vd.Voucher_ID = @VoucherID;

        OPEN dcur;
        FETCH NEXT FROM dcur INTO @D;

        WHILE @@FETCH_STATUS = 0
        BEGIN
            SELECT @DebtTotalAllocated = ISNULL(a.Allocated, 0), @DebtTotalOwed = d.Total_Owed
            FROM Debt_Register d
            LEFT JOIN dbo.fn_DebtAllocations('9999-12-31') a ON a.Debt_ID = d.Debt_ID
            WHERE d.Debt_ID = @D;

            IF @DebtTotalAllocated > @DebtTotalOwed
            BEGIN
                RAISERROR('Payment exceeds total owed for this debt. Transaction cancelled.',16,1);
                ROLLBACK TRANSACTION;
                CLOSE dcur; DEALLOCATE dcur;
                CLOSE cur; DEALLOCATE cur;
                RETURN;
            END

            IF @DebtTotalAllocated = @DebtTotalOwed
                UPDATE Debt_Register SET Is_Settled = 1 WHERE Debt_ID = @D;

            FETCH NEXT FROM dcur INTO @D;
        END

        CLOSE dcur;
        DEALLOCATE dcur;

        FETCH NEXT FROM cur INTO @VoucherID, @DebtID;
    END

    CLOSE cur;
    DEALLOCATE cur;
END;
GO

-- --------------------------------------------------------------------
-- STEP 5: VoucherDebts rules
-- --------------------------------------------------------------------
CREATE OR ALTER TRIGGER trg_VoucherDebts_Validate
ON VoucherDebts
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    -- Links are written once with the voucher and never edited afterwards.
    IF EXISTS (SELECT 1 FROM deleted)
    BEGIN
        RAISERROR('The debts on a voucher cannot be changed or removed once linked. Reject the voucher instead.',16,1);
        ROLLBACK TRANSACTION;
        RETURN;
    END

    -- Every debt must be owed to the voucher's own payee (one official).
    IF EXISTS (
        SELECT 1
        FROM inserted i
        INNER JOIN Vouchers v ON v.Voucher_ID = i.Voucher_ID
        INNER JOIN Debt_Register d ON d.Debt_ID = i.Debt_ID
        WHERE d.Creditor_Type <> 'Official'
           OR d.Official_link IS NULL
           OR v.Official_Link IS NULL
           OR d.Official_link <> v.Official_Link
    )
    BEGIN
        RAISERROR('All debts on one voucher must be owed to the same official, who must be the voucher payee.',16,1);
        ROLLBACK TRANSACTION;
        RETURN;
    END

    -- A debt is paid in full by its voucher.
    IF EXISTS (
        SELECT 1 FROM inserted i
        INNER JOIN Debt_Register d ON d.Debt_ID = i.Debt_ID
        WHERE i.Amount <> d.Total_Owed OR d.Is_Settled = 1
    )
    BEGIN
        RAISERROR('A voucher must pay each linked debt in full, and the debt must not already be settled.',16,1);
        ROLLBACK TRANSACTION;
        RETURN;
    END

    -- The debts on a voucher add up to the voucher amount.
    IF EXISTS (
        SELECT 1
        FROM (SELECT DISTINCT Voucher_ID FROM inserted) i
        INNER JOIN Vouchers v ON v.Voucher_ID = i.Voucher_ID
        WHERE v.Amount <> (SELECT SUM(vd.Amount) FROM VoucherDebts vd WHERE vd.Voucher_ID = i.Voucher_ID)
    )
    BEGIN
        RAISERROR('The debts on a voucher must add up to the voucher amount.',16,1);
        ROLLBACK TRANSACTION;
        RETURN;
    END

    -- A debt cannot be in two live vouchers at once (a rejected voucher no
    -- longer counts, so its debts can be raised again).
    IF EXISTS (
        SELECT 1
        FROM inserted i
        WHERE EXISTS (
                SELECT 1 FROM VoucherDebts o
                WHERE o.Debt_ID = i.Debt_ID AND o.Voucher_ID <> i.Voucher_ID
                  AND NOT EXISTS (SELECT 1 FROM Voucher_Approvals va WHERE va.Voucher_ID = o.Voucher_ID AND va.Approval_Status = 'Rejected'))
           OR EXISTS (
                SELECT 1 FROM Vouchers ov
                WHERE ov.Debt_Link = i.Debt_ID AND ov.Voucher_ID <> i.Voucher_ID
                  AND NOT EXISTS (SELECT 1 FROM Voucher_Approvals va WHERE va.Voucher_ID = ov.Voucher_ID AND va.Approval_Status = 'Rejected'))
    )
    BEGIN
        RAISERROR('One of these debts is already on another voucher that has not been rejected.',16,1);
        ROLLBACK TRANSACTION;
        RETURN;
    END
END;
GO

-- A voucher's amount and payee cannot be edited away from its debts.
CREATE OR ALTER TRIGGER trg_Vouchers_LockDebtVoucher
ON Vouchers
AFTER UPDATE
AS
BEGIN
    IF NOT (UPDATE(Amount) OR UPDATE(Official_Link))
        RETURN;

    IF EXISTS (
        SELECT 1
        FROM inserted i
        WHERE EXISTS (SELECT 1 FROM VoucherDebts vd WHERE vd.Voucher_ID = i.Voucher_ID)
          AND (
                i.Amount <> (SELECT SUM(vd.Amount) FROM VoucherDebts vd WHERE vd.Voucher_ID = i.Voucher_ID)
             OR EXISTS (
                    SELECT 1
                    FROM VoucherDebts vd
                    INNER JOIN Debt_Register d ON d.Debt_ID = vd.Debt_ID
                    WHERE vd.Voucher_ID = i.Voucher_ID
                      AND (i.Official_Link IS NULL OR d.Official_link <> i.Official_Link))
              )
    )
    BEGIN
        RAISERROR('This voucher pays debts, so its amount and payee cannot be changed. Reject it and raise a new one from Pay Debts.',16,1);
        ROLLBACK TRANSACTION;
        RETURN;
    END
END;
GO

-- --------------------------------------------------------------------
-- STEP 6: ActivityParticipants carries the debt a Pay decision creates
-- --------------------------------------------------------------------
ALTER TABLE ActivityParticipants ADD Debt_ID NVARCHAR(20) NULL
    CONSTRAINT FK_ActivityParticipants_Debt FOREIGN KEY REFERENCES Debt_Register(Debt_ID);
GO

CREATE UNIQUE INDEX UX_ActivityParticipants_Debt ON ActivityParticipants (Debt_ID) WHERE Debt_ID IS NOT NULL;
GO

ALTER TABLE ActivityParticipants DROP CONSTRAINT CK_ActivityParticipants_State;
GO

-- Each state carries exactly the columns that belong to it. A Pay line now
-- carries a Debt_ID (new) or a Voucher_ID (decided before this change), never both.
ALTER TABLE ActivityParticipants ADD CONSTRAINT CK_ActivityParticipants_State CHECK (
    (Decision IS NULL
        AND Reason IS NULL AND Budget_Link IS NULL AND Amount IS NULL AND Voucher_ID IS NULL AND Debt_ID IS NULL
        AND Decided_By IS NULL AND Decided_At IS NULL)
    OR
    (Decision = 'NotPayable'
        AND Reason IS NOT NULL AND LEN(LTRIM(RTRIM(Reason))) > 0
        AND Budget_Link IS NULL AND Amount IS NULL AND Voucher_ID IS NULL AND Debt_ID IS NULL
        AND Decided_By IS NOT NULL AND Decided_At IS NOT NULL)
    OR
    (Decision = 'Pay'
        AND Amount IS NOT NULL AND Amount > 0
        AND ((Debt_ID IS NOT NULL AND Voucher_ID IS NULL) OR (Debt_ID IS NULL AND Voucher_ID IS NOT NULL))
        AND Decided_By IS NOT NULL AND Decided_At IS NOT NULL)
);
GO

-- Same trigger as SQL/030, plus Debt_ID in the "decided lines never change" check.
CREATE OR ALTER TRIGGER trg_ActivityParticipants_ValidateDecision
ON ActivityParticipants
AFTER INSERT, UPDATE
AS
BEGIN
    IF EXISTS (
        SELECT 1 FROM inserted i
        WHERE i.Decision IS NOT NULL AND i.Decided_By = i.OfficialID
    )
    BEGIN
        RAISERROR('You cannot decide your own activity line; the other treasury officer must.',16,1);
        ROLLBACK TRANSACTION;
        RETURN;
    END

    IF EXISTS (
        SELECT 1
        FROM inserted i
        LEFT JOIN Ref_Officials o ON o.OfficialID = i.Decided_By
        WHERE i.Decision IS NOT NULL
          AND (o.OfficialID IS NULL OR o.IsCurrent = 0 OR o.Role NOT IN ('Treasurer', 'Deputy Treasurer'))
    )
    BEGIN
        RAISERROR('Only the current Treasurer or Deputy Treasurer may decide an activity line.',16,1);
        ROLLBACK TRANSACTION;
        RETURN;
    END

    IF EXISTS (
        SELECT 1
        FROM inserted i
        INNER JOIN deleted d ON d.Activity_ID = i.Activity_ID AND d.OfficialID = i.OfficialID
        WHERE d.Decision IS NOT NULL
          AND (ISNULL(i.Decision, '') <> ISNULL(d.Decision, '')
               OR ISNULL(i.Amount, -1) <> ISNULL(d.Amount, -1)
               OR ISNULL(i.Budget_Link, '') <> ISNULL(d.Budget_Link, '')
               OR ISNULL(i.Voucher_ID, '') <> ISNULL(d.Voucher_ID, '')
               OR ISNULL(i.Debt_ID, '') <> ISNULL(d.Debt_ID, '')
               OR ISNULL(i.Reason, '') <> ISNULL(d.Reason, ''))
    )
    BEGIN
        RAISERROR('This activity line has already been decided and can no longer be changed.',16,1);
        ROLLBACK TRANSACTION;
        RETURN;
    END
END;
GO

-- --------------------------------------------------------------------
-- Check (read-only): existing debt balances should look exactly as before.
-- --------------------------------------------------------------------
SELECT COUNT(*) AS DebtCount,
       SUM(Total_Owed) AS TotalOwed,
       SUM(TotalAllocated) AS TotalAllocated,
       SUM(Balance) AS TotalBalance
FROM DebtSummary;
GO
