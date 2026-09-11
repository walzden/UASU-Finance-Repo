USE UASU_Finance_Web;  -- adjust if your database name differs
GO

-- ====================================================================
-- Bootstrap the four initial logins (Chairman, Chapter Secretary,
-- Treasurer, Deputy Treasurer) when Users is empty and nobody can sign
-- in to use the in-app "Add Logins" page.
--
-- STEP 1: generate ONE hash first:
--
--     cd Tools/HashPasswordCli
--     dotnet run
--     Password to hash: test1234
--
-- STEP 2: paste that ONE hash into @Hash below - it's reused for all
-- four accounts, so there's only one copy-paste in this whole script
-- to get wrong, not four.
-- ====================================================================

DECLARE @Hash NVARCHAR(255) = 'PASTE-THE-HASH-FROM-HASHPASSWORDCLI-HERE';

-- Sanity check before touching anything: a real BCrypt hash is always
-- exactly 60 characters. If this isn't 60, STOP - fix @Hash above and
-- re-run from the top rather than continuing.
IF LEN(@Hash) <> 60
BEGIN
    RAISERROR('The hash in @Hash is not 60 characters - check you copied the whole thing, then re-run.', 16, 1);
    RETURN;
END

INSERT INTO Users (OfficialID, Username, PasswordHash, MustChangePassword)
SELECT OfficialID, Email, @Hash, 1
FROM Ref_Officials
WHERE Role = 'Chairman' AND IsCurrent = 1
  AND Email IS NOT NULL
  AND OfficialID NOT IN (SELECT OfficialID FROM Users);

INSERT INTO Users (OfficialID, Username, PasswordHash, MustChangePassword)
SELECT OfficialID, Email, @Hash, 1
FROM Ref_Officials
WHERE Role = 'Chapter Secretary' AND IsCurrent = 1
  AND Email IS NOT NULL
  AND OfficialID NOT IN (SELECT OfficialID FROM Users);

INSERT INTO Users (OfficialID, Username, PasswordHash, MustChangePassword)
SELECT OfficialID, Email, @Hash, 1
FROM Ref_Officials
WHERE Role = 'Treasurer' AND IsCurrent = 1
  AND Email IS NOT NULL
  AND OfficialID NOT IN (SELECT OfficialID FROM Users);

INSERT INTO Users (OfficialID, Username, PasswordHash, MustChangePassword)
SELECT OfficialID, Email, @Hash, 1
FROM Ref_Officials
WHERE Role = 'Deputy Treasurer' AND IsCurrent = 1
  AND Email IS NOT NULL
  AND OfficialID NOT IN (SELECT OfficialID FROM Users);

-- ====================================================================
-- Verify: one row per role you expect, HashLen = 60 on every row.
-- Anyone missing here means either no current officer holds that
-- role, or their Email is blank in Ref_Officials - both need fixing
-- in Ref_Officials directly, not in this script.
-- ====================================================================
SELECT u.Username, o.FullName, o.Role, LEN(u.PasswordHash) AS HashLen, u.MustChangePassword
FROM Users u
INNER JOIN Ref_Officials o ON o.OfficialID = u.OfficialID
ORDER BY o.Role;
