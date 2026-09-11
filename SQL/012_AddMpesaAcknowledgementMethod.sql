USE UASU_Finance_Web;  -- adjust if your database name differs
GO

-- ====================================================================
-- Payments/Acknowledge now offers "M-Pesa Confirmation" as a Method -
-- Safaricom's own transaction receipt (sent to the payer, naming the
-- registered account on the recipient's number), distinct from a
-- payee self-reporting receipt via SMS/WhatsApp/verbally. Same gap as
-- 011_AddWhatsAppAcknowledgementMethod.sql: the CHECK constraint on
-- Payment_Acknowledgements.Method doesn't know about it yet, so this
-- widens it the same way - look up whatever the constraint is
-- currently named, drop it, replace it with an explicitly-named one
-- that includes the new value. Safe to re-run.
-- ====================================================================

DECLARE @constraintName sysname;

SELECT @constraintName = cc.name
FROM sys.check_constraints cc
INNER JOIN sys.columns col
    ON col.object_id = cc.parent_object_id AND col.column_id = cc.parent_column_id
WHERE cc.parent_object_id = OBJECT_ID('Payment_Acknowledgements')
  AND col.name = 'Method'
  AND cc.name <> 'CK_Payment_Acknowledgements_Method';

IF @constraintName IS NOT NULL
BEGIN
    EXEC('ALTER TABLE Payment_Acknowledgements DROP CONSTRAINT [' + @constraintName + ']');
END
GO

IF EXISTS (
    SELECT 1 FROM sys.check_constraints
    WHERE name = 'CK_Payment_Acknowledgements_Method'
      AND definition NOT LIKE '%M-Pesa Confirmation%'
)
BEGIN
    ALTER TABLE Payment_Acknowledgements DROP CONSTRAINT CK_Payment_Acknowledgements_Method;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_Payment_Acknowledgements_Method')
BEGIN
    ALTER TABLE Payment_Acknowledgements ADD CONSTRAINT CK_Payment_Acknowledgements_Method
        CHECK (Method IN ('Signed Voucher', 'WhatsApp Confirmation', 'M-Pesa Confirmation', 'SMS Confirmation', 'Email Confirmation', 'Verbal - Witnessed', 'Other'));
END
GO
