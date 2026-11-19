-- No USE statement on purpose: run this with the target database chosen
-- explicitly (sqlcmd -d UASU_Finance_Test, or the SSMS database picker),
-- so it can't land on the live database by accident. Apply it to
-- UASU_Finance_Test first; run it on UASU_Finance_Web only once signed
-- off in dev, and take a backup before you do.

-- ====================================================================
-- A month can be certified only once.
--
-- MonthlyCertifications had only its Certification_ID primary key, so
-- nothing stopped the same Period_Year/Period_Month being certified a
-- second time (e.g. a double click, or two people on the page). The
-- Certification page now refuses it and offers a preview first; this
-- makes the database refuse it too.
--
-- STEP 1 lists any month already certified more than once - if it
-- returns rows, stop: decide which certification stands before going on.
-- ====================================================================

-- --------------------------------------------------------------------
-- STEP 1: Check for existing duplicates (expect no rows)
-- --------------------------------------------------------------------
SELECT Period_Year, Period_Month, COUNT(*) AS Certifications, STRING_AGG(Certification_ID, ', ') AS IDs
FROM MonthlyCertifications
GROUP BY Period_Year, Period_Month
HAVING COUNT(*) > 1;
GO

-- --------------------------------------------------------------------
-- STEP 2: One certification per period
-- --------------------------------------------------------------------
CREATE UNIQUE INDEX UX_MonthlyCertifications_Period
    ON MonthlyCertifications (Period_Year, Period_Month);
GO
