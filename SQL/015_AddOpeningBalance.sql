USE UASU_Finance_Web;  -- adjust if your database name differs
GO

-- ====================================================================
-- MonthlyCertifications' book balances are computed purely from
-- transactions this app has recorded (Payments/BankWithdrawals/
-- PaymentCharges) - with no starting point, that's "net movement since
-- day one," not the true cash/bank balance, so every certification
-- would show a huge, meaningless variance against whatever was
-- actually in the account before this app started tracking anything.
--
-- OpeningBalance is a single-row table (enforced by the Id=1 check,
-- same idea as a settings row) rather than an append-only log like
-- MonthlyCertifications - it's a one-time starting reference point,
-- not a transaction event, and if it's wrong it should just be
-- corrected in place rather than superseded by a new row.
--
-- Seeded here with zeros - CertificationService.GetBookBalancesAsync
-- treats the balances as of this row's As_Of_Date, so adding zero
-- changes nothing until real opening figures are entered through the
-- app. As_Of_Date defaults to the earliest Payment on file (the
-- moment just before which this app has no transaction history at
-- all) - update it if the real figures are as of a different date.
-- ====================================================================

CREATE TABLE OpeningBalance (
    Id INT NOT NULL PRIMARY KEY CHECK (Id = 1),
    As_Of_Date DATE NOT NULL,
    Cash_Balance DECIMAL(12,2) NOT NULL,
    Bank_Balance DECIMAL(12,2) NOT NULL,
    Set_By NVARCHAR(20) NULL,
    Set_Date DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_OpeningBalance_Official FOREIGN KEY (Set_By) REFERENCES Ref_Officials(OfficialID)
);
GO

INSERT INTO OpeningBalance (Id, As_Of_Date, Cash_Balance, Bank_Balance, Set_By)
VALUES (1, ISNULL((SELECT MIN(Payment_Date) FROM Payments), GETDATE()), 0, 0, NULL);
GO
