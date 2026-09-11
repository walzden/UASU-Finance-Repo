USE UASU_Finance_Web;  -- adjust if your database name differs
GO

-- ====================================================================
-- Optional evidence attachment for a payee acknowledgement - e.g. a
-- screenshot of a WhatsApp confirmation message. Stored directly in
-- SQL Server (not on disk) so it rides along with the same backup/
-- restore process as the rest of the financial record, rather than
-- needing a separate file-storage backup story that's easy to
-- accidentally leave out of a restore plan.
-- ====================================================================

ALTER TABLE Payment_Acknowledgements ADD
    Attachment_Data VARBINARY(MAX) NULL,
    Attachment_FileName NVARCHAR(255) NULL,
    Attachment_ContentType NVARCHAR(100) NULL;
GO
