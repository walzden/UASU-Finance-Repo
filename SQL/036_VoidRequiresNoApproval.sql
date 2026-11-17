-- No USE statement on purpose: run this with the target database chosen
-- explicitly (sqlcmd -d UASU_Finance_Test, or the SSMS database picker),
-- so it can't land on the live database by accident. Apply it to
-- UASU_Finance_Test first; run it on UASU_Finance_Web only once signed
-- off in dev, and take a backup before you do. Run AFTER 033.

-- ====================================================================
-- Treasury can't void a voucher an approver has already approved.
--
-- SQL/033 let any unpaid voucher be voided, including one the Chairman
-- or Chapter Secretary had already approved. That let the Treasurer or
-- Deputy undo an approver's decision on their own, while the same
-- voucher showed as Locked for editing.
--
-- New rule: a voucher can be voided only if no approver has approved it,
-- unless it has since been rejected. So:
--   - no decision yet                  -> can be voided
--   - rejected (by either approver)    -> can be voided (e.g. a duplicate
--                                         the approver threw out)
--   - approved by one or both, not
--     rejected                         -> refused; ask an approver to
--                                         reject it, then void it
--
-- trg_Vouchers_Void is re-created with this one extra check; everything
-- else in it is unchanged from SQL/033.
-- ====================================================================

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

    -- An approver's approval stands until an approver rejects it.
    IF EXISTS (
        SELECT 1 FROM inserted i
        WHERE i.Voided_Date IS NOT NULL
          AND EXISTS (SELECT 1 FROM Voucher_Approvals va
                      WHERE va.Voucher_ID = i.Voucher_ID AND va.Approval_Status = 'Approved')
          AND NOT EXISTS (SELECT 1 FROM Voucher_Approvals va
                          WHERE va.Voucher_ID = i.Voucher_ID AND va.Approval_Status = 'Rejected')
    )
    BEGIN
        RAISERROR('An approver has already approved this voucher, so it cannot be voided. Ask the Chairman or Chapter Secretary to reject it; it can then be voided from the rejected list on New Voucher.',16,1);
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
