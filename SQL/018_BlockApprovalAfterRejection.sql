USE UASU_Finance_Web;  -- adjust if your database name differs
GO

-- ====================================================================
-- trg_Voucher_Approvals_Validate previously only blocked a duplicate
-- 'Approved' row for the SAME role on the same voucher - it never
-- considered rejection at all, so once (say) the Chapter Secretary
-- rejected a voucher, the Chairman could still separately approve it.
-- FullyApproved (requires both roles = Approved) correctly stayed 0
-- forever in that case, so it could never become payable - but it left
-- a contradictory audit trail (one Approved, one Rejected) for a
-- voucher that had already vanished from both approvers' queues, with
-- nothing telling the Chairman their approval was moot.
--
-- Ring-fences this two ways:
--   1. Once ANY role has rejected a voucher, no further 'Approved' row
--      can be recorded against it at all (this trigger).
--   2. GetPendingApprovalsAsync (VoucherService.cs) now also pulls a
--      voucher from the OTHER approver's queue the moment it's
--      rejected, not just the rejecting approver's own queue - so
--      nobody is ever prompted to vote on something already dead.
-- A second, independent rejection (both roles reject) is still allowed
-- - that's not ambiguous, just two people agreeing.
-- ====================================================================

CREATE OR ALTER TRIGGER trg_Voucher_Approvals_Validate
ON Voucher_Approvals
AFTER INSERT
AS
BEGIN
    -- Reject if the approver isn't a current Chairman/Chapter Secretary
    IF EXISTS (
        SELECT 1
        FROM inserted i
        INNER JOIN Ref_Officials o ON o.OfficialID = i.Approver_ID
        WHERE o.Role NOT IN ('Chairman','Chapter Secretary')
           OR o.IsCurrent = 0
    )
    BEGIN
        RAISERROR('Only the current Chairman or Chapter Secretary may approve a voucher.',16,1);
        ROLLBACK TRANSACTION;
        RETURN;
    END

    -- Reject a duplicate 'Approved' row for the same voucher + role
    IF EXISTS (
        SELECT 1
        FROM inserted i
        INNER JOIN Ref_Officials o ON o.OfficialID = i.Approver_ID
        WHERE i.Approval_Status = 'Approved'
        AND EXISTS (
            SELECT 1
            FROM Voucher_Approvals va
            INNER JOIN Ref_Officials o2 ON o2.OfficialID = va.Approver_ID
            WHERE va.Voucher_ID = i.Voucher_ID
              AND o2.Role = o.Role
              AND va.Approval_Status = 'Approved'
              AND va.Approval_ID <> i.Approval_ID
        )
    )
    BEGIN
        RAISERROR('This voucher already has an approved sign-off from that role.',16,1);
        ROLLBACK TRANSACTION;
        RETURN;
    END

    -- Reject an 'Approved' row for a voucher any role has already
    -- rejected - a rejection is final, regardless of who votes next.
    IF EXISTS (
        SELECT 1
        FROM inserted i
        WHERE i.Approval_Status = 'Approved'
          AND EXISTS (
              SELECT 1 FROM Voucher_Approvals va
              WHERE va.Voucher_ID = i.Voucher_ID
                AND va.Approval_Status = 'Rejected'
                AND va.Approval_ID <> i.Approval_ID
          )
    )
    BEGIN
        RAISERROR('This voucher was already rejected and can no longer be approved.',16,1);
        ROLLBACK TRANSACTION;
        RETURN;
    END
END;
GO
