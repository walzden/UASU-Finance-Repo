USE UASU_Finance_Web;  -- adjust if your database name differs
GO

-- ====================================================================
-- Most income arrives as a bank deposit, M-Pesa, or cash payment that's
-- already landed in the account by the time anyone gets around to
-- entering the voucher - routing it through Chairman/Chapter Secretary
-- approval added a step with nothing left to actually approve. This
-- migration makes Income vouchers payment-eligible immediately, the
-- same way Is_Legacy already does for pre-workflow vouchers, and closes
-- the one gap that combination opens up: an Income voucher can now
-- reach a paid state (via the app's own auto-payment option on New
-- Voucher, or an ordinary Record Payment) without ever picking up a
-- Voucher_Approvals row, so the existing edit-lock trigger - which
-- only checked for one of those - needs to also check for a payment.
-- ====================================================================

-- 1. VoucherPaymentEligible: add Income alongside the existing
--    Is_Legacy / FullyApproved conditions. trg_PaymentAllocations_
--    RequireApproval reads from this view, so this is the one place
--    that actually grants Income vouchers payment eligibility - the
--    app layer (VoucherService.GetPendingApprovalsAsync excluding
--    Income, GetApprovedVouchersAsync including it) mirrors this but
--    doesn't enforce it.
CREATE OR ALTER VIEW VoucherPaymentEligible AS
SELECT v.Voucher_ID
FROM Vouchers v
LEFT JOIN VoucherApprovalStatus vas ON vas.Voucher_ID = v.Voucher_ID
WHERE v.Is_Legacy = 1 OR ISNULL(vas.FullyApproved, 0) = 1 OR v.Transaction_Type = 'Income';
GO

-- 2. trg_Vouchers_BlockEditAfterApproval (SQL/020): originally only
--    checked for a Voucher_Approvals row. An auto-paid Income voucher
--    never gets one of those, so without this it would stay editable
--    forever even after being marked paid - extend the same lock to
--    cover "already has a payment on file" too.
--
--    IMPORTANT - this also fixes a pre-existing bug in SQL/020 that has
--    nothing to do with Income: trg_PaymentStatusUpdate (on
--    PaymentAllocations) always does an UPDATE Vouchers SET Current_
--    Status = ... after every payment, for every voucher type. With
--    nested triggers ON (the server default, confirmed still on here),
--    that UPDATE re-fires this trigger. SQL/020's version checked ANY
--    update against "does an approval exist", so recording a payment
--    against an already-approved voucher would immediately block
--    itself and roll back - discovered while testing this migration,
--    before any real payment had been recorded against an approved
--    voucher since SQL/020 was created (2026-08-27). This version only
--    evaluates the block when a genuinely user-editable field is part
--    of the UPDATE (via UPDATE(column)) - trg_PaymentStatusUpdate's
--    Current_Status-only UPDATE never touches any of those columns, so
--    it now passes through untouched, while a real edit attempt (which
--    always sets every one of these columns - see VoucherService.
--    UpdateVoucherAsync) is still blocked exactly as before.
CREATE OR ALTER TRIGGER trg_Vouchers_BlockEditAfterApproval
ON Vouchers
AFTER UPDATE
AS
BEGIN
    IF NOT (
        UPDATE(VoucherDate) OR UPDATE(Transaction_Type) OR UPDATE(Budget_Link) OR
        UPDATE(Official_Link) OR UPDATE(Supplier_Link) OR UPDATE(Amount) OR
        UPDATE([Description]) OR UPDATE(Manual_Payee_Name) OR UPDATE(Manual_Payee_Address) OR
        UPDATE(Payee_Category) OR UPDATE(Debt_link) OR UPDATE(Is_Travel_Expense) OR
        UPDATE(Traveler_Name) OR UPDATE(Travel_From) OR UPDATE(Travel_To) OR
        UPDATE(Travel_Mode) OR UPDATE(Travel_Date) OR UPDATE(Travel_Reason) OR
        UPDATE(Supersedes_Voucher_ID)
    )
        RETURN;

    IF EXISTS (
        SELECT 1
        FROM inserted i
        WHERE EXISTS (
            SELECT 1 FROM Voucher_Approvals va
            WHERE va.Voucher_ID = i.Voucher_ID
        )
        OR EXISTS (
            SELECT 1 FROM PaymentAllocations pa
            WHERE pa.Voucher_ID = i.Voucher_ID
        )
    )
    BEGIN
        RAISERROR('This voucher already has an approval decision or payment on file and can no longer be edited - reject it first if it needs correction, then resubmit as a new voucher.',16,1);
        ROLLBACK TRANSACTION;
        RETURN;
    END
END;
GO
