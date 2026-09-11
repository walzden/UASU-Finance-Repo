USE UASU_Finance_Web;  -- adjust if your database name differs
GO

-- ====================================================================
-- Email verification gate for first-time sign-in. Users.Username is
-- the officer's email address (set at seed time in SeedDefaultLogins/
-- HashPasswordCli), but nothing ever confirmed it's a real inbox the
-- officer actually controls - a typo'd or dummy address would still
-- work fine as a login username. This adds a one-time code challenge
-- that must be cleared before a freshly-seeded account can reach
-- Change Password, so a bad address gets caught (no code arrives)
-- before the officer is holding a working login built on it.
-- ====================================================================

ALTER TABLE Users ADD
    EmailVerified BIT NOT NULL CONSTRAINT DF_Users_EmailVerified DEFAULT 0,
    VerificationCodeHash NVARCHAR(255) NULL,
    VerificationCodeExpiry DATETIME NULL;
GO

-- Existing accounts that have already changed their password once
-- already proved (in practice, over however long they've been signing
-- in) that Username is a reachable address - don't retroactively gate
-- them. Only accounts still sitting on MustChangePassword = 1 (never
-- really used yet) go through the new check.
UPDATE Users SET EmailVerified = 1 WHERE MustChangePassword = 0;
GO
