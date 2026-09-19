USE UASU_Finance_Web;  -- adjust if your database name differs
GO

-- ====================================================================
-- Activity log: officials record what they do on behalf of the union in
-- one place (instead of WhatsApp messages), the treasury later decides
-- line by line what is paid, and paid lines become ordinary Expense
-- vouchers that go through the normal Chairman + Chapter Secretary
-- approval.
--
-- Activities            one row per event/activity, logged by any official
-- ActivityParticipants  one row per official who took part - this is the
--                       unit the treasury decides on (Pay / Not payable),
--                       and where the voucher link is stored
-- ActivityEvidence      photos/PDFs supporting an activity
--
-- Nothing here changes Vouchers or the approval workflow; a paid line just
-- points at the voucher created for it (ActivityParticipants.Voucher_ID),
-- which is what stops the same line being paid twice.
--
-- Depends on SQL/029 (Vouchers.Description widened to 1000) because one
-- voucher can now list several activities with their dates.
--
-- Run against a COPY of the database first.
-- ====================================================================

CREATE SEQUENCE ActivitySeq
    AS INT
    START WITH 1
    INCREMENT BY 1;
GO

CREATE TABLE Activities (
    Activity_ID NVARCHAR(20) NOT NULL PRIMARY KEY
        DEFAULT ('ACT-' + RIGHT('0000' + CONVERT(VARCHAR(6), NEXT VALUE FOR ActivitySeq), 4)),
    Activity_Date DATE NOT NULL,
    Category NVARCHAR(100) NOT NULL,
    Title NVARCHAR(200) NOT NULL,
    Details NVARCHAR(2000) NOT NULL,
    Place NVARCHAR(200) NULL,
    Days_Spent DECIMAL(4,1) NOT NULL
        CONSTRAINT CK_Activities_DaysSpent CHECK (Days_Spent > 0 AND Days_Spent <= 31),
    Others_Involved NVARCHAR(300) NULL,
    Authorised_By NVARCHAR(100) NOT NULL,
    -- What the official says they paid out of pocket, for the treasury's
    -- information only - it is NOT added to any voucher automatically.
    Expenses_Amount DECIMAL(18,2) NOT NULL
        CONSTRAINT DF_Activities_Expenses DEFAULT 0
        CONSTRAINT CK_Activities_Expenses CHECK (Expenses_Amount >= 0),
    Expenses_Note NVARCHAR(300) NULL,
    -- Whoever typed it in. Not necessarily one of the participants: a
    -- colleague can record for officials who have not signed in yet.
    Logged_By NVARCHAR(20) NOT NULL
        CONSTRAINT FK_Activities_LoggedBy FOREIGN KEY REFERENCES Ref_Officials(OfficialID),
    Logged_At DATETIME NOT NULL CONSTRAINT DF_Activities_LoggedAt DEFAULT GETDATE()
);
GO

CREATE INDEX IX_Activities_Date ON Activities (Activity_Date);
GO

CREATE TABLE ActivityParticipants (
    Activity_ID NVARCHAR(20) NOT NULL
        CONSTRAINT FK_ActivityParticipants_Activity FOREIGN KEY REFERENCES Activities(Activity_ID),
    OfficialID NVARCHAR(20) NOT NULL
        CONSTRAINT FK_ActivityParticipants_Official FOREIGN KEY REFERENCES Ref_Officials(OfficialID),

    -- NULL = the treasury has not decided yet.
    Decision NVARCHAR(12) NULL
        CONSTRAINT CK_ActivityParticipants_Decision CHECK (Decision IN ('Pay', 'NotPayable')),
    Reason NVARCHAR(500) NULL,            -- the policy reason when NotPayable; shown to the official
    Budget_Link NVARCHAR(20) NULL
        CONSTRAINT FK_ActivityParticipants_Budget FOREIGN KEY REFERENCES Ref_BudgetCodes(Budget_ID),
    Amount DECIMAL(18,2) NULL,
    Voucher_ID NVARCHAR(20) NULL
        CONSTRAINT FK_ActivityParticipants_Voucher FOREIGN KEY REFERENCES Vouchers(Voucher_ID),
    Decided_By NVARCHAR(20) NULL
        CONSTRAINT FK_ActivityParticipants_DecidedBy FOREIGN KEY REFERENCES Ref_Officials(OfficialID),
    Decided_At DATETIME NULL,

    CONSTRAINT PK_ActivityParticipants PRIMARY KEY (Activity_ID, OfficialID),

    -- Each state carries exactly the columns that belong to it.
    CONSTRAINT CK_ActivityParticipants_State CHECK (
        (Decision IS NULL
            AND Reason IS NULL AND Budget_Link IS NULL AND Amount IS NULL AND Voucher_ID IS NULL
            AND Decided_By IS NULL AND Decided_At IS NULL)
        OR
        (Decision = 'NotPayable'
            AND Reason IS NOT NULL AND LEN(LTRIM(RTRIM(Reason))) > 0
            AND Budget_Link IS NULL AND Amount IS NULL AND Voucher_ID IS NULL
            AND Decided_By IS NOT NULL AND Decided_At IS NOT NULL)
        OR
        (Decision = 'Pay'
            AND Budget_Link IS NOT NULL AND Amount IS NOT NULL AND Amount > 0
            AND Voucher_ID IS NOT NULL
            AND Decided_By IS NOT NULL AND Decided_At IS NOT NULL)
    )
);
GO

CREATE INDEX IX_ActivityParticipants_Official ON ActivityParticipants (OfficialID);
CREATE INDEX IX_ActivityParticipants_Voucher ON ActivityParticipants (Voucher_ID);
GO

CREATE TABLE ActivityEvidence (
    Evidence_ID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Activity_ID NVARCHAR(20) NOT NULL
        CONSTRAINT FK_ActivityEvidence_Activity FOREIGN KEY REFERENCES Activities(Activity_ID),
    File_Name NVARCHAR(260) NOT NULL,
    Content_Type NVARCHAR(100) NOT NULL,
    File_Data VARBINARY(MAX) NOT NULL,
    Uploaded_At DATETIME NOT NULL CONSTRAINT DF_ActivityEvidence_UploadedAt DEFAULT GETDATE()
);
GO

CREATE INDEX IX_ActivityEvidence_Activity ON ActivityEvidence (Activity_ID);
GO

-- --------------------------------------------------------------------
-- Separation of duties, enforced here as well as in the app (same idea as
-- trg_Voucher_Approvals_Validate): only the current Treasurer or Deputy
-- Treasurer may record a decision, and nobody may decide their own line -
-- the Treasurer's lines are decided by the Deputy Treasurer and the other
-- way round. A line that already has a voucher can never be changed.
-- --------------------------------------------------------------------
CREATE OR ALTER TRIGGER trg_ActivityParticipants_ValidateDecision
ON ActivityParticipants
AFTER INSERT, UPDATE
AS
BEGIN
    IF EXISTS (
        SELECT 1 FROM inserted i
        WHERE i.Decision IS NOT NULL AND i.Decided_By = i.OfficialID
    )
    BEGIN
        RAISERROR('You cannot decide your own activity line; the other treasury officer must.',16,1);
        ROLLBACK TRANSACTION;
        RETURN;
    END

    IF EXISTS (
        SELECT 1
        FROM inserted i
        LEFT JOIN Ref_Officials o ON o.OfficialID = i.Decided_By
        WHERE i.Decision IS NOT NULL
          AND (o.OfficialID IS NULL OR o.IsCurrent = 0 OR o.Role NOT IN ('Treasurer', 'Deputy Treasurer'))
    )
    BEGIN
        RAISERROR('Only the current Treasurer or Deputy Treasurer may decide an activity line.',16,1);
        ROLLBACK TRANSACTION;
        RETURN;
    END

    IF EXISTS (
        SELECT 1
        FROM inserted i
        INNER JOIN deleted d ON d.Activity_ID = i.Activity_ID AND d.OfficialID = i.OfficialID
        WHERE d.Decision IS NOT NULL
          AND (ISNULL(i.Decision, '') <> ISNULL(d.Decision, '')
               OR ISNULL(i.Amount, -1) <> ISNULL(d.Amount, -1)
               OR ISNULL(i.Budget_Link, '') <> ISNULL(d.Budget_Link, '')
               OR ISNULL(i.Voucher_ID, '') <> ISNULL(d.Voucher_ID, '')
               OR ISNULL(i.Reason, '') <> ISNULL(d.Reason, ''))
    )
    BEGIN
        RAISERROR('This activity line has already been decided and can no longer be changed.',16,1);
        ROLLBACK TRANSACTION;
        RETURN;
    END
END;
GO
