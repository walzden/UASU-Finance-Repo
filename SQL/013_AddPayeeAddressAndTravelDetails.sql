USE UASU_Finance_Web;  -- adjust if your database name differs
GO

-- ====================================================================
-- Labour Relations (Accounts) Regulations, reg. 6(1)(e): every payment
-- voucher must record "the name and address of the recipient of the
-- payment". Address wasn't captured anywhere - added here as:
--   - Ref_Officials.Address / Ref_Suppliers.Address for payees with a
--     persistent reference row (populated once, reused by every
--     voucher paid to that official/supplier via the join already used
--     for their name).
--   - Vouchers.Manual_Payee_Address for one-time payees, who have no
--     reference row - same pattern as Manual_Payee_Name.
--
-- Ref_Officials/Ref_Suppliers have no management page in this app
-- (they're populated outside it), so their Address columns start out
-- NULL for everyone already on file - existing officials/suppliers'
-- addresses need to be filled in directly via SQL until/unless this
-- app grows an edit page for them.
-- ====================================================================

ALTER TABLE Ref_Officials ADD Address NVARCHAR(255) NULL;
GO

ALTER TABLE Ref_Suppliers ADD Address NVARCHAR(255) NULL;
GO

ALTER TABLE Vouchers ADD Manual_Payee_Address NVARCHAR(255) NULL;
GO

-- Only bites for an Expense paid to a one-time payee (Manual_Payee_Name
-- set) - reg. 6 is about money paid OUT of union funds, so this
-- deliberately doesn't require an address on Income entries (Donor /
-- Income Source), which reuse the same Manual_Payee_Name field for who
-- gave the union money, not who received a payment.
ALTER TABLE Vouchers ADD CONSTRAINT CK_Vouchers_ManualPayeeAddress
    CHECK (
        NOT (Transaction_Type = 'Expense' AND Manual_Payee_Name IS NOT NULL)
        OR Manual_Payee_Address IS NOT NULL
    );
GO

-- ====================================================================
-- Reg. 6(1)(d): where a payment is for travelling expenses, the
-- voucher must record the traveller's name, the places travelled from
-- and to, the mode of transport, the reason for the journey, and its
-- date. Previously all of this lived, inconsistently, inside the free-
-- text Description field. Is_Travel_Expense is an independent flag
-- (not tied to Payee_Category or Budget_Link) since a travel payment
-- can go to any kind of payee under any budget code.
-- ====================================================================

ALTER TABLE Vouchers ADD
    Is_Travel_Expense BIT NOT NULL CONSTRAINT DF_Vouchers_IsTravelExpense DEFAULT 0,
    Traveler_Name NVARCHAR(150) NULL,
    Travel_From NVARCHAR(150) NULL,
    Travel_To NVARCHAR(150) NULL,
    Travel_Mode NVARCHAR(100) NULL,
    Travel_Date DATE NULL,
    Travel_Reason NVARCHAR(255) NULL;
GO

ALTER TABLE Vouchers ADD CONSTRAINT CK_Vouchers_TravelDetails
    CHECK (
        Is_Travel_Expense = 0
        OR (
            Traveler_Name IS NOT NULL AND Travel_From IS NOT NULL AND Travel_To IS NOT NULL
            AND Travel_Mode IS NOT NULL AND Travel_Date IS NOT NULL AND Travel_Reason IS NOT NULL
        )
    );
GO
