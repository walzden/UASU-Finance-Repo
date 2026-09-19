USE UASU_Finance_Web;  -- adjust if your database name differs
GO

-- ====================================================================
-- Monthly honoraria are a fixed amount per union position, so instead
-- of typing one voucher per official every month, Pages/Honoraria/
-- Generate.cshtml creates them all in one go from this table.
--
-- Rates are keyed by position name, matched against Ref_Officials.Role
-- (free text there, so the spelling must agree - the Generate page lists
-- any current official whose Role has no rate here as "No rate" rather
-- than silently skipping them). The rates are edited afterwards from
-- Pages/Honoraria/Rates.cshtml, not by re-running this script.
--
-- Nothing else in the approval workflow changes: generated vouchers are
-- ordinary Expense vouchers and still need Chairman + Chapter Secretary
-- approval like any other.
-- ====================================================================

CREATE TABLE Ref_HonorariaRates (
    Role NVARCHAR(100) NOT NULL PRIMARY KEY,
    Amount DECIMAL(18,2) NOT NULL
        CONSTRAINT CK_HonorariaRates_Amount CHECK (Amount > 0)
);
GO

INSERT INTO Ref_HonorariaRates (Role, Amount) VALUES
    ('Chapter Secretary',        9000),
    ('Chairman',                 8000),
    ('Deputy Chapter Secretary', 6500),
    ('Vice Chairman',            6000),
    ('Treasurer',                7000),
    ('Deputy Treasurer',         6000),
    ('Organizing Secretary',     6000),
    ('Trustee',                  5000),
    ('Delegate',                 5000);
GO
