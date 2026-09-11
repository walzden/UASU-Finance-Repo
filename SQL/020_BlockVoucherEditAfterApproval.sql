USE UASU_Finance_Web;  -- adjust if your database name differs
GO

-- ====================================================================
-- Pages/Vouchers/Pending.cshtml lets the Treasurer/Deputy Treasurer fix
-- a typo or missing field on a voucher before it gets rejected for it -
-- but only while nobody has ruled on it yet. Nothing before this
-- migration actually stopped a Vouchers row from being updated after a
-- Voucher_Approvals row already existed for it: a Chairman could
-- approve based on one amount/payee, then have those substantive
-- fields silently changed out from under their decision.
--
-- This blocks ANY update to a Vouchers row once ANY Voucher_Approvals
-- row exists for it - approved or rejected, either role. That matches
-- the app's existing append-only pattern for a rejected voucher
-- (SQL/017: corrections happen by creating a new voucher that
-- supersedes the old one, never by editing it in place) and extends
-- the same rule to the "not yet decided by either role" case, which
-- previously had no rule at all.
-- ====================================================================

CREATE OR ALTER TRIGGER trg_Vouchers_BlockEditAfterApproval
ON Vouchers
AFTER UPDATE
AS
BEGIN
    IF EXISTS (
        SELECT 1
        FROM inserted i
        WHERE EXISTS (
            SELECT 1 FROM Voucher_Approvals va
            WHERE va.Voucher_ID = i.Voucher_ID
        )
    )
    BEGIN
        RAISERROR('This voucher already has an approval decision on file and can no longer be edited - reject it first if it needs correction, then resubmit as a new voucher.',16,1);
        ROLLBACK TRANSACTION;
        RETURN;
    END
END;
GO
