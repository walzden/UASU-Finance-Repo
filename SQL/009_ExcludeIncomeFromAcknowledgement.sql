USE UASU_Finance_Web;  -- adjust if your database name differs
GO

-- ====================================================================
-- PaymentsAwaitingAcknowledgement previously listed every payment with
-- no Payment_Acknowledgements row, regardless of direction. That's
-- wrong for Income payments (e.g. a donor grant deposited into the
-- union's account) - there's no external payee to acknowledge
-- receiving money the union itself received, so those rows sat in the
-- Payments/Acknowledge dropdown with nothing meaningful to record
-- against them.
--
-- This narrows the view to payments that fund at least one Expense
-- voucher, matching what "acknowledgement of receipt" actually means:
-- confirmation from whoever the union paid out to.
-- ====================================================================

CREATE OR ALTER VIEW PaymentsAwaitingAcknowledgement AS
SELECT
    p.Payment_ID,
    p.Payment_Date,
    p.Payment_Mode,
    p.Amount_Paid,
    p.[Description]
FROM Payments p
LEFT JOIN Payment_Acknowledgements pa ON pa.Payment_ID = p.Payment_ID
WHERE pa.Payment_ID IS NULL
  AND EXISTS (
        SELECT 1
        FROM PaymentAllocations al
        INNER JOIN Vouchers v ON v.Voucher_ID = al.Voucher_ID
        WHERE al.Payment_ID = p.Payment_ID
          AND v.Transaction_Type = 'Expense'
  );
GO
