USE UASU_Finance_Web;  -- adjust if your database name differs
GO

-- ====================================================================
-- Labour Relations (Accounts) Regulations, reg. 8(2)/9(2): the
-- treasurer must balance the cash book at the end of every month and
-- certify (sign) that the balances agree with actual cash in hand and
-- cash at the bank. Nothing in this app previously recorded that this
-- monthly close ever happened.
--
-- MonthlyCertifications is an append-only log, same spirit as
-- Voucher_Approvals/Payment_Acknowledgements - it doesn't block or
-- overwrite a prior certification for the same period; if a mistake
-- needs correcting, a fresh certification for that period is just
-- another row, and the history stays intact rather than being edited
-- away.
--
-- Book_Cash_Balance/Book_Bank_Balance are computed by the app from
-- Payments at certification time (cumulative to the selected month-
-- end); Actual_Cash_On_Hand/Actual_Bank_Balance are what the treasurer
-- physically counted / read off the bank statement. Both get stored
-- side by side rather than just a pass/fail flag, so a variance is
-- visible in the record itself, not just implied by its absence.
-- ====================================================================

CREATE SEQUENCE seq_CertificationID AS INT START WITH 1 INCREMENT BY 1;
GO

CREATE TABLE MonthlyCertifications (
    Certification_ID NVARCHAR(20) NOT NULL PRIMARY KEY
        DEFAULT ('CERT-' + CONVERT(VARCHAR(4), DATEPART(YEAR, GETDATE())) + '-'
                 + RIGHT('0000' + CONVERT(VARCHAR(4), NEXT VALUE FOR seq_CertificationID), 4)),
    Period_Year INT NOT NULL,
    Period_Month INT NOT NULL,
    Book_Cash_Balance DECIMAL(12,2) NOT NULL,
    Book_Bank_Balance DECIMAL(12,2) NOT NULL,
    Actual_Cash_On_Hand DECIMAL(12,2) NOT NULL,
    Actual_Bank_Balance DECIMAL(12,2) NOT NULL,
    Certified_By NVARCHAR(20) NOT NULL,
    Certified_Date DATETIME NOT NULL DEFAULT GETDATE(),
    Notes NVARCHAR(500) NULL,
    CONSTRAINT CK_MonthlyCertifications_Month CHECK (Period_Month BETWEEN 1 AND 12),
    CONSTRAINT FK_MonthlyCertifications_Official FOREIGN KEY (Certified_By) REFERENCES Ref_Officials(OfficialID)
);
GO
