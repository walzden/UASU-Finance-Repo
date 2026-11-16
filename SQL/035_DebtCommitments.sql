-- No USE statement on purpose: run this with the target database chosen
-- explicitly (sqlcmd -d UASU_Finance_Test, or the SSMS database picker),
-- so it can't land on the live database by accident. Apply it to
-- UASU_Finance_Test first; run it on UASU_Finance_Web only once signed
-- off in dev, and take a backup before you do. Run AFTER 033.

-- ====================================================================
-- Stop a debt being put on more vouchers than it is worth.
--
-- Until now a debt only dropped out of New Voucher's "Pay toward an
-- existing debt" list once it was PAID (DebtSummary.Balance = 0). A
-- voucher raised for it but still awaiting approval left it listed, so
-- the Treasurer and Deputy Treasurer could each raise one for the same
-- debt.
--
-- What this adds
--   1. fn_DebtCommitments(): ONE definition of how much of each debt is
--      already on a live voucher - every voucher that is not rejected and
--      not voided, paid or not. A voucher linked by Vouchers.Debt_Link
--      counts its full Amount; one linked through VoucherDebts (Pay
--      Debts) counts that debt's line. Same two link types as
--      fn_DebtAllocations (SQL/031). New Voucher reads it to list only
--      debts with something left to raise.
--   2. trg_Vouchers_DebtNotOverCommitted: refuses any insert or edit that
--      would put a debt on vouchers worth more than Total_Owed. It locks
--      the debt row first, so two people raising a voucher for the same
--      debt at the same moment are handled one after the other, and the
--      second is refused.
--
-- A debt may still be paid in instalments across several vouchers, as
-- long as together they don't exceed what is owed. Rejecting or voiding
-- a voucher frees its amount again.
--
-- Nothing existing is altered. Each step is its own batch (GO); if a
-- step errors, stop and send the message rather than continuing.
-- ====================================================================

-- --------------------------------------------------------------------
-- STEP 1: fn_DebtCommitments
-- --------------------------------------------------------------------
CREATE OR ALTER FUNCTION dbo.fn_DebtCommitments ()
RETURNS TABLE
AS
RETURN
    SELECT x.Debt_ID, SUM(x.Amount) AS Committed
    FROM (
        -- Linked through Vouchers.Debt_Link only (vouchers that also have
        -- VoucherDebts rows are counted in the branch below instead).
        SELECT v.Debt_Link AS Debt_ID, v.Amount
        FROM Vouchers v
        WHERE v.Debt_Link IS NOT NULL
          AND v.Voided_Date IS NULL
          AND NOT EXISTS (SELECT 1 FROM Voucher_Approvals va
                          WHERE va.Voucher_ID = v.Voucher_ID AND va.Approval_Status = 'Rejected')
          AND NOT EXISTS (SELECT 1 FROM VoucherDebts vd WHERE vd.Voucher_ID = v.Voucher_ID)

        UNION ALL

        -- Linked through VoucherDebts (Pay Debts): that debt's own line.
        SELECT vd.Debt_ID, vd.Amount
        FROM VoucherDebts vd
        INNER JOIN Vouchers v ON v.Voucher_ID = vd.Voucher_ID
        WHERE v.Voided_Date IS NULL
          AND NOT EXISTS (SELECT 1 FROM Voucher_Approvals va
                          WHERE va.Voucher_ID = v.Voucher_ID AND va.Approval_Status = 'Rejected')
    ) x
    GROUP BY x.Debt_ID;
GO

-- --------------------------------------------------------------------
-- STEP 2: Refuse a voucher that over-commits a debt
-- --------------------------------------------------------------------
CREATE OR ALTER TRIGGER trg_Vouchers_DebtNotOverCommitted
ON Vouchers
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT (UPDATE(Debt_link) OR UPDATE(Amount))
        RETURN;

    IF NOT EXISTS (SELECT 1 FROM inserted WHERE Debt_Link IS NOT NULL)
        RETURN;

    -- Serialise vouchers for the same debt: a second statement touching
    -- these debts waits here until this one commits, then sees its row.
    DECLARE @lock INT;
    SELECT @lock = COUNT(*)
    FROM Debt_Register d WITH (UPDLOCK, HOLDLOCK)
    WHERE d.Debt_ID IN (SELECT Debt_Link FROM inserted WHERE Debt_Link IS NOT NULL);

    DECLARE @msg NVARCHAR(400);
    SELECT TOP 1 @msg =
        'Debt ' + d.Debt_ID + ' is owed Sh. ' + FORMAT(d.Total_Owed, 'N2')
        + ' but vouchers not rejected or voided would now cover Sh. ' + FORMAT(c.Committed, 'N2')
        + '. Check Pending Vouchers for a voucher already raised for this debt.'
    FROM Debt_Register d
    INNER JOIN dbo.fn_DebtCommitments() c ON c.Debt_ID = d.Debt_ID
    WHERE d.Debt_ID IN (SELECT Debt_Link FROM inserted WHERE Debt_Link IS NOT NULL)
      AND c.Committed > d.Total_Owed;

    IF @msg IS NOT NULL
    BEGIN
        RAISERROR(@msg, 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END
END;
GO

-- --------------------------------------------------------------------
-- STEP 3: Check - debts already over-committed before this script
-- (expect none; any listed need a voucher rejected or voided)
-- --------------------------------------------------------------------
SELECT d.Debt_ID, d.Total_Owed, c.Committed
FROM Debt_Register d
INNER JOIN dbo.fn_DebtCommitments() c ON c.Debt_ID = d.Debt_ID
WHERE c.Committed > d.Total_Owed;
GO
