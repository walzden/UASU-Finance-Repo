-- No USE statement on purpose: run this with the target database chosen
-- explicitly (sqlcmd -d UASU_Finance_Test, or the SSMS database picker),
-- so it can't land on the live database by accident. Apply it to
-- UASU_Finance_Test first; run it on UASU_Finance_Web only once the
-- feature has been signed off in dev, and take a backup before you do.

-- ====================================================================
-- Voiding a voucher (e.g. one raised twice by mistake).
--
-- Voucher numbers come from VoucherSeq and are never reused or deleted:
-- an auditor expects an unbroken run, and a missing number looks worse
-- than one plainly marked void. Until now the only way to kill a voucher
-- was for an approver to reject it, which left it on the "Rejected
-- vouchers awaiting correction" list for good. This adds a proper,
-- final Voided state with who did it, when and why.
--
-- Rules (enforced here, not just in the app)
--   - A voucher can be voided only while it has NO payment against it,
--     and it must record who voided it and a reason.
--   - A cash float retirement voucher cannot be voided - reject it
--     instead, which releases its receipts to be retired again
--     (SQL/032).
--   - Voided is final: a voided voucher cannot be edited, un-voided,
--     approved, rejected or paid.
--   - A voided voucher no longer counts as "live", so the debts it was
--     paying can be put on a new voucher (trg_VoucherDebts_Validate is
--     re-created below with that one change).
--
-- What this changes
--   1. Vouchers: Voided_Date, Voided_By, Void_Reason; Current_Status
--      gains 'Voided' (the old unnamed CHECK is replaced by a named one).
--   2. VoucherPaymentEligible excludes voided vouchers (so
--      trg_PaymentAllocations_RequireApproval refuses them too).
--   3. New triggers: trg_Vouchers_Void, trg_Voucher_Approvals_BlockVoided,
--      trg_PaymentAllocations_BlockVoided.
--   4. trg_VoucherDebts_Validate re-created (voided = not live).
--
-- Nothing else existing is altered. Each step is its own batch (GO); if
-- a step errors, stop and send the message rather than continuing.
-- ====================================================================

-- --------------------------------------------------------------------
-- STEP 1: Columns
-- --------------------------------------------------------------------
ALTER TABLE Vouchers ADD
    Voided_Date DATETIME NULL,
    Voided_By NVARCHAR(20) NULL
        CONSTRAINT FK_Vouchers_VoidedBy FOREIGN KEY REFERENCES Ref_Officials(OfficialID),
    Void_Reason NVARCHAR(300) NULL;
GO

-- --------------------------------------------------------------------
-- STEP 2: Current_Status may now be 'Voided'
-- The original CHECK was created without a name, so find it by its
-- column and drop it, then add a named replacement.
-- --------------------------------------------------------------------
DECLARE @ck SYSNAME, @sql NVARCHAR(400);
SELECT @ck = cc.name
FROM sys.check_constraints cc
INNER JOIN sys.columns c ON c.object_id = cc.parent_object_id AND c.column_id = cc.parent_column_id
WHERE cc.parent_object_id = OBJECT_ID('Vouchers') AND c.name = 'Current_Status';

IF @ck IS NOT NULL
BEGIN
    SET @sql = N'ALTER TABLE Vouchers DROP CONSTRAINT ' + QUOTENAME(@ck) + N';';
    EXEC sp_executesql @sql;
END
GO

ALTER TABLE Vouchers ADD CONSTRAINT CK_Vouchers_CurrentStatus
    CHECK (Current_Status IN ('Pending', 'Partially Paid', 'Paid', 'Voided'));
GO

-- Voided status and the void details always go together.
ALTER TABLE Vouchers ADD CONSTRAINT CK_Vouchers_Voided CHECK (
    (Voided_Date IS NULL AND Voided_By IS NULL AND Void_Reason IS NULL AND Current_Status <> 'Voided')
 OR (Voided_Date IS NOT NULL AND Voided_By IS NOT NULL AND Current_Status = 'Voided'
     AND Void_Reason IS NOT NULL AND LEN(LTRIM(RTRIM(Void_Reason))) > 0)
);
GO

-- --------------------------------------------------------------------
-- STEP 3: A voided voucher is never payment-eligible
-- (same definition as before, plus the Voided_Date filter)
-- --------------------------------------------------------------------
CREATE OR ALTER VIEW VoucherPaymentEligible AS
SELECT v.Voucher_ID
FROM Vouchers v
LEFT JOIN VoucherApprovalStatus vas ON vas.Voucher_ID = v.Voucher_ID
WHERE v.Voided_Date IS NULL
  AND (v.Is_Legacy = 1 OR ISNULL(vas.FullyApproved, 0) = 1 OR v.Transaction_Type = 'Income');
GO

-- --------------------------------------------------------------------
-- STEP 4: Voiding rules on Vouchers
-- --------------------------------------------------------------------
CREATE OR ALTER TRIGGER trg_Vouchers_Void
ON Vouchers
AFTER UPDATE
AS
BEGIN
    -- Voided is final: nothing about a voided voucher may change.
    IF EXISTS (SELECT 1 FROM deleted WHERE Voided_Date IS NOT NULL)
    BEGIN
        RAISERROR('This voucher has been voided. A voided voucher is final and cannot be changed.',16,1);
        ROLLBACK TRANSACTION;
        RETURN;
    END

    IF NOT UPDATE(Voided_Date)
        RETURN;

    -- Only an unpaid voucher can be voided.
    IF EXISTS (
        SELECT 1 FROM inserted i
        WHERE i.Voided_Date IS NOT NULL
          AND EXISTS (SELECT 1 FROM PaymentAllocations pa WHERE pa.Voucher_ID = i.Voucher_ID)
    )
    BEGIN
        RAISERROR('This voucher already has a payment against it, so it cannot be voided.',16,1);
        ROLLBACK TRANSACTION;
        RETURN;
    END

    -- Float retirement vouchers are corrected by rejection, which releases
    -- their receipts (SQL/032) - voiding would strand them.
    IF EXISTS (
        SELECT 1 FROM inserted i
        WHERE i.Voided_Date IS NOT NULL
          AND EXISTS (SELECT 1 FROM CashFloatReceipts r WHERE r.Voucher_ID = i.Voucher_ID)
    )
    BEGIN
        RAISERROR('This voucher retires cash float receipts and cannot be voided. Ask an approver to reject it instead; the receipts can then be retired again from the float.',16,1);
        ROLLBACK TRANSACTION;
        RETURN;
    END
END;
GO

-- --------------------------------------------------------------------
-- STEP 5: No approval decision on a voided voucher
-- --------------------------------------------------------------------
CREATE OR ALTER TRIGGER trg_Voucher_Approvals_BlockVoided
ON Voucher_Approvals
AFTER INSERT, UPDATE
AS
BEGIN
    IF EXISTS (
        SELECT 1 FROM inserted i
        INNER JOIN Vouchers v ON v.Voucher_ID = i.Voucher_ID
        WHERE v.Voided_Date IS NOT NULL
    )
    BEGIN
        RAISERROR('This voucher has been voided and can no longer be approved or rejected.',16,1);
        ROLLBACK TRANSACTION;
        RETURN;
    END
END;
GO

-- --------------------------------------------------------------------
-- STEP 6: No payment on a voided voucher
-- (VoucherPaymentEligible already stops this via
-- trg_PaymentAllocations_RequireApproval; this gives a clearer message.)
-- --------------------------------------------------------------------
CREATE OR ALTER TRIGGER trg_PaymentAllocations_BlockVoided
ON PaymentAllocations
AFTER INSERT, UPDATE
AS
BEGIN
    IF EXISTS (
        SELECT 1 FROM inserted i
        INNER JOIN Vouchers v ON v.Voucher_ID = i.Voucher_ID
        WHERE v.Voided_Date IS NOT NULL
    )
    BEGIN
        RAISERROR('This voucher has been voided and cannot be paid.',16,1);
        ROLLBACK TRANSACTION;
        RETURN;
    END
END;
GO

-- --------------------------------------------------------------------
-- STEP 7: trg_VoucherDebts_Validate - identical to SQL/031 except the
-- last rule: a voided voucher, like a rejected one, no longer holds its
-- debts, so they can be put on a new voucher.
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

    -- A debt cannot be in two live vouchers at once (a rejected or voided
    -- voucher no longer counts, so its debts can be raised again).
    IF EXISTS (
        SELECT 1
        FROM inserted i
        WHERE EXISTS (
                SELECT 1 FROM VoucherDebts o
                INNER JOIN Vouchers ov ON ov.Voucher_ID = o.Voucher_ID
                WHERE o.Debt_ID = i.Debt_ID AND o.Voucher_ID <> i.Voucher_ID
                  AND ov.Voided_Date IS NULL
                  AND NOT EXISTS (SELECT 1 FROM Voucher_Approvals va WHERE va.Voucher_ID = o.Voucher_ID AND va.Approval_Status = 'Rejected'))
           OR EXISTS (
                SELECT 1 FROM Vouchers ov
                WHERE ov.Debt_Link = i.Debt_ID AND ov.Voucher_ID <> i.Voucher_ID
                  AND ov.Voided_Date IS NULL
                  AND NOT EXISTS (SELECT 1 FROM Voucher_Approvals va WHERE va.Voucher_ID = ov.Voucher_ID AND va.Approval_Status = 'Rejected'))
    )
    BEGIN
        RAISERROR('One of these debts is already on another voucher that has not been rejected or voided.',16,1);
        ROLLBACK TRANSACTION;
        RETURN;
    END
END;
GO

-- --------------------------------------------------------------------
-- STEP 8: Check
-- --------------------------------------------------------------------
SELECT COUNT(*) AS VoidedVouchers FROM Vouchers WHERE Voided_Date IS NOT NULL;  -- expect 0
GO
