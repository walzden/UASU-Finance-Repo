USE UASU_Finance_Web;  -- adjust if your database name differs
GO

-- ====================================================================
-- PaymentCharges (and the existing Payments/Charges page) only cover
-- charges CAUSED BY a specific payment - an M-Pesa transaction fee, a
-- cheque charge - always tied to a Payment_ID. A bank ledger fee,
-- excise duty, or account maintenance charge isn't caused by any
-- payment or withdrawal at all; it's the bank debiting the account
-- directly. Nothing in the current schema can represent that: even
-- though PaymentCharges.Payment_ID happens to be nullable, Bank
-- Reconciliation's CTEs all key off Payment_ID/Withdrawal_ID, so a
-- charge with neither would silently vanish from every report instead
-- of reconciling against anything.
--
-- AccountCharges is a separate, standalone table for exactly this case
-- - tied only to a Bank_Account and a date, with its own running total
-- per account (Pages/Banking/AccountCharges.cshtml), deliberately kept
-- out of BankReconciliation/Withdrawal Breakdown since there's no
-- specific withdrawal to net it against.
-- ====================================================================

CREATE SEQUENCE AccountChargeSeq
    AS INT
    START WITH 1
    INCREMENT BY 1;
GO

CREATE TABLE AccountCharges (
    Charge_ID NVARCHAR(40) NOT NULL PRIMARY KEY
        DEFAULT ('ACG-' + RIGHT('000' + CONVERT(VARCHAR(3), NEXT VALUE FOR AccountChargeSeq), 3)),
    Charge_Date DATE NOT NULL,
    Bank_Account NVARCHAR(200) NOT NULL,
    Charge_Type NVARCHAR(100) NOT NULL
        CONSTRAINT CK_AccountCharges_ChargeType
        CHECK (Charge_Type IN ('Account Ledger Fee', 'Excise Duty', 'Maintenance Fee', 'Other')),
    Charge_Amount DECIMAL(18,2) NOT NULL
        CONSTRAINT CK_AccountCharges_ChargeAmount CHECK (Charge_Amount > 0),
    Notes NVARCHAR(510) NULL
);
GO
