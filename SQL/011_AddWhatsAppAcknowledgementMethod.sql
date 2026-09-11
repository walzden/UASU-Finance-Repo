USE UASU_Finance_Web;  -- adjust if your database name differs
GO

-- ====================================================================
-- Payments/Acknowledge now offers "WhatsApp Confirmation" as a Method
-- (see Pages/Payments/Acknowledge.cshtml), but the existing CHECK
-- constraint on Payment_Acknowledgements.Method - auto-named by SQL
-- Server since it was never given an explicit name at creation - only
-- allowed Signed Voucher / SMS Confirmation / Email Confirmation /
-- Verbal - Witnessed / Other. Inserting a WhatsApp acknowledgement
-- fails at the database layer until this runs.
--
-- SQL Server can't ALTER a CHECK constraint's definition in place, so
-- this looks up whatever the constraint is actually named on this
-- database, drops it, and replaces it with an explicitly-named one
-- that includes the new value. Safe to re-run, and doesn't depend on
-- the auto-generated name matching across different copies of the
-- database.
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

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_Payment_Acknowledgements_Method')
BEGIN
    ALTER TABLE Payment_Acknowledgements ADD CONSTRAINT CK_Payment_Acknowledgements_Method
        CHECK (Method IN ('Signed Voucher', 'WhatsApp Confirmation', 'SMS Confirmation', 'Email Confirmation', 'Verbal - Witnessed', 'Other'));
END
GO
