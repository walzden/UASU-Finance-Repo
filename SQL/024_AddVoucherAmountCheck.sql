USE UASU_Finance_Web;  -- adjust if your database name differs
GO

-- ====================================================================
-- VoucherInputModel.Amount already has [Range(0.01, double.MaxValue)]
-- (a UX nicety, per CLAUDE.md), but nothing at the database layer ever
-- enforced Amount > 0 - unlike every other Vouchers business rule
-- (payee category, travel details, transaction type), which is
-- re-checked here independently of the app. This closes that gap the
-- same way.
-- ====================================================================

ALTER TABLE Vouchers ADD CONSTRAINT CK_Vouchers_Amount CHECK (Amount > 0);
GO
