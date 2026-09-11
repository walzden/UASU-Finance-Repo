USE UASU_Finance_Web;  -- adjust if your database name differs
GO

-- ====================================================================
-- AccountCharges.Charge_Type used "Excise Duty" (SQL/022) - renamed to
-- "Excise Charges" to match the wording actually printed on the bank
-- statement, so entries here read the same as the line item they're
-- transcribing.
-- ====================================================================

ALTER TABLE AccountCharges DROP CONSTRAINT CK_AccountCharges_ChargeType;
GO

UPDATE AccountCharges SET Charge_Type = 'Excise Charges' WHERE Charge_Type = 'Excise Duty';
GO

ALTER TABLE AccountCharges ADD CONSTRAINT CK_AccountCharges_ChargeType
    CHECK (Charge_Type IN ('Account Ledger Fee', 'Excise Charges', 'Maintenance Fee', 'Other'));
GO
