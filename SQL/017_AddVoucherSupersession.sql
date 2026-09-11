USE UASU_Finance_Web;  -- adjust if your database name differs
GO

-- ====================================================================
-- When a voucher is rejected for a data-quality reason (wrong amount,
-- wrong description, etc.), there's no "Edit Voucher" page and no way
-- for the rejecting approver to revisit their own decision through the
-- app (GetPendingApprovalsAsync excludes any voucher the current
-- approver has already ruled on, forever). Rather than trying to
-- reopen the same voucher for a re-vote - which would mean adding
-- voucher editing and breaking the one-decision-per-approver pattern
-- used everywhere else - a corrected voucher is a fresh voucher that
-- references the one it replaces.
--
-- This keeps the original rejected voucher exactly as submitted
-- (matches Voucher_Approvals/Payment_Acknowledgements/
-- MonthlyCertifications, which are all append-only too) while still
-- giving a queryable trail from "this was rejected" to "here's what
-- replaced it".
-- ====================================================================

ALTER TABLE Vouchers ADD Supersedes_Voucher_ID NVARCHAR(20) NULL;
GO

ALTER TABLE Vouchers ADD CONSTRAINT FK_Vouchers_Supersedes
    FOREIGN KEY (Supersedes_Voucher_ID) REFERENCES Vouchers(Voucher_ID);
GO

ALTER TABLE Vouchers ADD CONSTRAINT CK_Vouchers_NoSelfSupersede
    CHECK (Supersedes_Voucher_ID IS NULL OR Supersedes_Voucher_ID <> Voucher_ID);
GO
