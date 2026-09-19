USE UASU_Finance_Web;  -- adjust if your database name differs
GO

-- ====================================================================
-- Vouchers.Description was 255 characters. A voucher that aggregates
-- several activities (for example one official's committee meetings for
-- the month, each with its date) needs more room, so this widens it to
-- 1000, in step with the app's [MaxLength(1000)] on VoucherInputModel.
--
-- Payments.Description is widened too: when an Income voucher is marked
-- as received, VoucherService.CreateVoucherAsync copies the voucher's
-- description straight into the new Payments row. If Payments stayed at
-- 255, a long Income description would fail with "String or binary data
-- would be truncated" instead of saving.
--
-- Safe to re-run: a column already 1000 or wider (or MAX) is skipped.
-- The column's own type (varchar / nvarchar) and NULL / NOT NULL setting
-- are read from the database and kept as they are.
--
-- RUN ORDER
--   1. Run STEP 1 on its own to see what is there now (nothing changes).
--   2. Run STEP 2 and STEP 3 against a COPY of the database first.
--   3. Then run them on UASU_Finance_Web.
--
-- ALTER COLUMN will stop with a clear error if something blocks it, such
-- as a SCHEMABINDING view or an index that would exceed its key-size
-- limit on the column. That leaves the database unchanged (STEP 2 is one
-- transaction), so read the message and tell me what it says.
-- ====================================================================

-- --------------------------------------------------------------------
-- STEP 1: what is there now (read-only)
-- --------------------------------------------------------------------
SELECT TABLE_NAME, COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE COLUMN_NAME = 'Description' AND TABLE_NAME IN ('Vouchers', 'Payments');
GO

-- --------------------------------------------------------------------
-- STEP 2: widen the columns
-- --------------------------------------------------------------------
SET XACT_ABORT ON;
BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE @tbl SYSNAME, @type NVARCHAR(20), @len INT, @nullable NVARCHAR(3), @sql NVARCHAR(MAX);

    DECLARE cols CURSOR LOCAL FAST_FORWARD FOR
        SELECT TABLE_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH, IS_NULLABLE
        FROM INFORMATION_SCHEMA.COLUMNS
        WHERE COLUMN_NAME = 'Description'
          AND TABLE_NAME IN ('Vouchers', 'Payments')
          AND DATA_TYPE IN ('varchar', 'nvarchar');

    OPEN cols;
    FETCH NEXT FROM cols INTO @tbl, @type, @len, @nullable;
    WHILE @@FETCH_STATUS = 0
    BEGIN
        -- -1 means MAX, which is already wide enough.
        IF @len <> -1 AND @len < 1000
        BEGIN
            SET @sql = N'ALTER TABLE dbo.' + QUOTENAME(@tbl) + N' ALTER COLUMN [Description] ' + @type + N'(1000) '
                     + CASE WHEN @nullable = 'YES' THEN N'NULL' ELSE N'NOT NULL' END + N';';
            EXEC sp_executesql @sql;
            PRINT 'Widened ' + @tbl + '.Description from ' + CAST(@len AS VARCHAR(10)) + ' to 1000.';
        END
        ELSE
            PRINT 'Skipped ' + @tbl + '.Description (already ' + CAST(@len AS VARCHAR(10)) + ').';

        FETCH NEXT FROM cols INTO @tbl, @type, @len, @nullable;
    END
    CLOSE cols;
    DEALLOCATE cols;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO

-- --------------------------------------------------------------------
-- STEP 3: refresh the views so they pick up the new column width
-- (views that read Description keep the old width in their metadata
-- until refreshed). A view that cannot be refreshed is reported and
-- skipped rather than stopping the script.
-- --------------------------------------------------------------------
DECLARE @view NVARCHAR(400);
DECLARE views CURSOR LOCAL FAST_FORWARD FOR
    SELECT QUOTENAME(SCHEMA_NAME(schema_id)) + N'.' + QUOTENAME(name) FROM sys.views;
OPEN views;
FETCH NEXT FROM views INTO @view;
WHILE @@FETCH_STATUS = 0
BEGIN
    BEGIN TRY
        EXEC sp_refreshview @view;
    END TRY
    BEGIN CATCH
        PRINT 'Could not refresh ' + @view + ': ' + ERROR_MESSAGE();
    END CATCH;
    FETCH NEXT FROM views INTO @view;
END
CLOSE views;
DEALLOCATE views;
GO

-- Confirm (read-only): both should now show 1000.
SELECT TABLE_NAME, COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS
WHERE COLUMN_NAME = 'Description' AND TABLE_NAME IN ('Vouchers', 'Payments');
GO
