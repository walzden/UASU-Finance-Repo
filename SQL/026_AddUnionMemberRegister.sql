USE UASU_Finance_Web;  -- adjust if your database name differs
GO

-- ====================================================================
-- RTU (Registrar of Trade Unions) requires an annual member register
-- showing every contributor and which months they paid union dues in.
-- The union has two kinds of contributor: full Members (dues-paying)
-- and Agency Payers (non-members covered by the CBA who pay an agency
-- fee under Kenya's Labour Relations Act instead of joining) - MMU's
-- existing register keeps these as two lists per year with the same
-- shape (PF No., Name, Jan..Dec, Total), so the schema mirrors that
-- while normalizing months into rows instead of 12 wide columns.
--
-- Union_Contributors is the permanent roster, keyed by a generated
-- union membership number (Contributor_ID) - separate from PF_No,
-- which is MMU's own payroll number and just the natural link back to
-- HR/payroll data, not something this app controls the format of.
--
-- Union_Registrations is one row per contributor per year - just the
-- (contributor, year) pairing itself, nothing else.
--
-- Union_Contributions is one row per registration per month, and
-- Membership_Type lives HERE rather than on Union_Registrations -
-- someone can switch between Agency Payer and full Member mid-year
-- (confirmed against the real 2025 data: 13 people appear in both the
-- Member and Agency Payer sheets for 2025, with non-overlapping
-- months in each), so a single type per contributor per year can't
-- represent that. "Which months did this person contribute as a
-- Member" for the year-end register is just "which months have a row
-- here for their registration with Membership_Type = 'Member'."
--
-- No per-member address column: every contributor's RTU address is
-- the same fixed MMU chapter address, so it's hardcoded on the report
-- itself (same pattern as the HEAD OFFICE block on Form R/Form Q)
-- rather than duplicated on every row.
-- ====================================================================

CREATE SEQUENCE ContributorSeq
    AS INT
    START WITH 1
    INCREMENT BY 1;
GO

CREATE TABLE Union_Contributors (
    Contributor_ID NVARCHAR(20) NOT NULL PRIMARY KEY
        DEFAULT ('MMU-' + RIGHT('0000' + CONVERT(VARCHAR(4), NEXT VALUE FOR ContributorSeq), 4)),
    PF_No NVARCHAR(20) NOT NULL,
    Full_Name NVARCHAR(200) NOT NULL,
    Created_Date DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT UQ_Union_Contributors_PFNo UNIQUE (PF_No)
);
GO

CREATE TABLE Union_Registrations (
    Registration_ID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Contributor_ID NVARCHAR(20) NOT NULL
        CONSTRAINT FK_Union_Registrations_Contributor REFERENCES Union_Contributors(Contributor_ID),
    Register_Year INT NOT NULL,
    CONSTRAINT UQ_Union_Registrations UNIQUE (Contributor_ID, Register_Year)
);
GO

CREATE TABLE Union_Contributions (
    Contribution_ID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Registration_ID INT NOT NULL
        CONSTRAINT FK_Union_Contributions_Registration REFERENCES Union_Registrations(Registration_ID),
    Contribution_Month TINYINT NOT NULL
        CONSTRAINT CK_Union_Contributions_Month CHECK (Contribution_Month BETWEEN 1 AND 12),
    Membership_Type NVARCHAR(10) NOT NULL
        CONSTRAINT CK_Union_Contributions_Type CHECK (Membership_Type IN ('Member', 'Agency')),
    Amount DECIMAL(18,2) NOT NULL
        CONSTRAINT CK_Union_Contributions_Amount CHECK (Amount > 0),
    CONSTRAINT UQ_Union_Contributions UNIQUE (Registration_ID, Contribution_Month)
);
GO
