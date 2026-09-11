USE UASU_Finance_Web;  -- adjust if your database name differs
GO

-- ====================================================================
-- VoucherApprovalStatus only ever checked Approval_Status = 'Approved'
-- - a rejection left ChairmanApproved/ChapterSecretaryApproved at 0,
-- indistinguishable from "hasn't voted yet". Approvals/Index.cshtml
-- (and VoucherService.GetPendingApprovalsAsync behind it) reads those
-- two columns to show the "Other approver" status to whichever
-- approver hasn't decided yet - so a rejection by one role was
-- completely invisible to the other, who'd see "None yet" for a
-- voucher that had already been turned down.
--
-- Purely additive: ChairmanApproved/ChapterSecretaryApproved/
-- FullyApproved keep their exact existing meaning (VoucherPaymentEligible,
-- and the payment-eligibility trigger behind it, read only
-- FullyApproved, so this doesn't touch payment eligibility at all).
-- The two new *Rejected columns just make the already-recorded
-- rejection visible.
-- ====================================================================

CREATE OR ALTER VIEW VoucherApprovalStatus AS
SELECT
    v.Voucher_ID,
    MAX(CASE WHEN o.Role = 'Chairman' AND va.Approval_Status = 'Approved'
             THEN 1 ELSE 0 END) AS ChairmanApproved,
    MAX(CASE WHEN o.Role = 'Chapter Secretary' AND va.Approval_Status = 'Approved'
             THEN 1 ELSE 0 END) AS ChapterSecretaryApproved,
    MAX(CASE WHEN o.Role = 'Chairman' AND va.Approval_Status = 'Rejected'
             THEN 1 ELSE 0 END) AS ChairmanRejected,
    MAX(CASE WHEN o.Role = 'Chapter Secretary' AND va.Approval_Status = 'Rejected'
             THEN 1 ELSE 0 END) AS ChapterSecretaryRejected,
    CASE WHEN
        MAX(CASE WHEN o.Role = 'Chairman' AND va.Approval_Status = 'Approved'
                 THEN 1 ELSE 0 END)
      + MAX(CASE WHEN o.Role = 'Chapter Secretary' AND va.Approval_Status = 'Approved'
                 THEN 1 ELSE 0 END) = 2
    THEN 1 ELSE 0 END AS FullyApproved
FROM Vouchers v
LEFT JOIN Voucher_Approvals va ON va.Voucher_ID = v.Voucher_ID
LEFT JOIN Ref_Officials o ON o.OfficialID = va.Approver_ID
GROUP BY v.Voucher_ID;
GO
