USE UASU_Finance_Web;  -- adjust if your database name differs
GO

-- ====================================================================
-- trg_PaymentStatusUpdate already rejects allocating more to a VOUCHER
-- than that voucher's own Amount. There was no equivalent check in the
-- other direction: nothing stopped total allocations against a single
-- PAYMENT from exceeding that payment's Amount_Paid. The app now
-- filters a payment out of the "add another voucher" dropdown once
-- it's fully allocated, but that's a page-load-time check, not an
-- atomic one - two people acting on the same payment at once could
-- still slip past it. This trigger is the real backstop.
-- ====================================================================

CREATE TRIGGER trg_PaymentAllocations_ValidatePaymentTotal
ON PaymentAllocations
AFTER INSERT
AS
BEGIN
    IF EXISTS (
        SELECT 1
        FROM inserted i
        INNER JOIN Payments p ON p.Payment_ID = i.Payment_ID
        WHERE (
            SELECT SUM(pa.Allocated_Amount)
            FROM PaymentAllocations pa
            WHERE pa.Payment_ID = i.Payment_ID
        ) > p.Amount_Paid
    )
    BEGIN
        RAISERROR('Total allocations for this payment would exceed its Amount_Paid.',16,1);
        ROLLBACK TRANSACTION;
        RETURN;
    END
END;
GO
