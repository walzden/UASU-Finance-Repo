USE UASU_Finance_Web;  -- adjust if your database name differs
GO

-- ====================================================================
-- Legacy voucher exemption from the approval workflow.
-- Run this once against your live database.
-- ====================================================================

-- ------------------------------------------------------------------
-- 1. Flag on Vouchers. Defaults to 0 - the app's CreateVoucherAsync
-- never sets this, so every new voucher raised through the app is
-- automatically subject to the approval workflow. Is_Legacy can only
-- ever become 1 via the backfill below or a deliberate manual UPDATE.
-- ------------------------------------------------------------------
ALTER TABLE Vouchers ADD Is_Legacy BIT NOT NULL DEFAULT 0;
GO

-- ------------------------------------------------------------------
-- 2. Backfill: a voucher is legacy if it already has at least one
-- payment recorded against it but has never been through
-- Voucher_Approvals at all. That's the actual signature of "this was
-- paid before the approval workflow existed" - not a date guess, and
-- it won't misfire on a brand-new voucher that's simply pending its
-- first approval (that voucher has no payment yet either).
-- ------------------------------------------------------------------
UPDATE v
SET Is_Legacy = 1
FROM Vouchers v
WHERE EXISTS (
        SELECT 1 FROM PaymentAllocations pa WHERE pa.Voucher_ID = v.Voucher_ID
      )
  AND NOT EXISTS (
        SELECT 1 FROM Voucher_Approvals va WHERE va.Voucher_ID = v.Voucher_ID
      );
GO

-- Sanity check before moving on - review this list before trusting the trigger change below.
SELECT Voucher_ID, VoucherDate, Amount, Current_Status, Is_Legacy
FROM Vouchers
WHERE Is_Legacy = 1
ORDER BY VoucherDate;
GO

-- ------------------------------------------------------------------
-- 3. Single source of truth for "can this voucher be paid": fully
-- approved, or explicitly grandfathered in as legacy. Both the
-- trigger and the app's payment form read from this one view, so the
-- rule is never duplicated (and never drifts) between the two.
-- ------------------------------------------------------------------
CREATE VIEW VoucherPaymentEligible AS
SELECT v.Voucher_ID
FROM Vouchers v
LEFT JOIN VoucherApprovalStatus vas ON vas.Voucher_ID = v.Voucher_ID
WHERE v.Is_Legacy = 1 OR ISNULL(vas.FullyApproved, 0) = 1;
GO

-- ------------------------------------------------------------------
-- 4. Replace the payment-blocking trigger to check the view above
-- instead of VoucherApprovalStatus directly - this is the only
-- change needed for legacy vouchers to be payable (e.g. a legacy
-- voucher that was only partially paid before the cutover, and now
-- needs a follow-up payment recorded).
-- ------------------------------------------------------------------
DROP TRIGGER trg_PaymentAllocations_RequireApproval;
GO

CREATE TRIGGER trg_PaymentAllocations_RequireApproval
ON PaymentAllocations
AFTER INSERT
AS
BEGIN
    IF EXISTS (
        SELECT 1
        FROM inserted i
        WHERE i.Voucher_ID NOT IN (SELECT Voucher_ID FROM VoucherPaymentEligible)
    )
    BEGIN
        RAISERROR('Cannot pay a voucher that is neither fully approved nor marked as legacy.',16,1);
        ROLLBACK TRANSACTION;
        RETURN;
    END
END;
GO
