using Dapper;
using Microsoft.Data.SqlClient;
using UASU_VoucherApprovals.Data;
using UASU_VoucherApprovals.Models;

namespace UASU_VoucherApprovals.Services;

public interface IVoucherService
{
    Task<IEnumerable<PendingApproval>> GetPendingApprovalsAsync(string role, string officialId);
    Task SubmitApprovalAsync(string voucherId, string officialId, string status, string? comments);
    Task<IEnumerable<BulkApprovalResult>> BulkApproveAsync(IEnumerable<string> voucherIds, string officialId, string? comments);

    Task<IEnumerable<SimpleOption>> GetBudgetCodesAsync();
    Task<IEnumerable<OfficialOption>> GetOfficialsAsync();
    Task<IEnumerable<SimpleOption>> GetSuppliersAsync();
    Task<IEnumerable<RejectedVoucherRow>> GetRejectedVouchersAwaitingCorrectionAsync();
    Task<string> CreateVoucherAsync(VoucherInputModel input);

    Task<IEnumerable<PendingVoucherRow>> GetPendingVouchersAsync();
    Task UpdateVoucherAsync(VoucherInputModel input, string voucherId);

    Task<IEnumerable<ApprovedVoucherRow>> GetApprovedVouchersAsync(DateTime start, DateTime end);
    Task<IEnumerable<VoucherApprovalActionRow>> GetVoucherApprovalActionsAsync(DateTime start, DateTime end);
    Task<IEnumerable<VoucherApprovalActionRow>> GetVoucherApprovalActionsByIdsAsync(IEnumerable<string> voucherIds);
    Task<IEnumerable<VoucherPaymentRow>> GetVoucherPaymentsAsync(DateTime start, DateTime end);

    Task<IEnumerable<SimpleOption>> GetFullyApprovedUnpaidVouchersAsync();
    Task<IEnumerable<ApprovedUnpaidVoucherRow>> GetApprovedUnpaidVouchersDetailedAsync();
    Task<string> RecordPaymentAsync(PaymentInputModel input);

    Task<IEnumerable<PaymentAcknowledgementOption>> GetPaymentsAwaitingAcknowledgementAsync();
    Task RecordAcknowledgementAsync(AcknowledgementInputModel input);

    Task<IEnumerable<AcknowledgementRow>> GetRecentAcknowledgementsAsync();
    Task<IEnumerable<AcknowledgementRow>> GetAllAcknowledgementsAsync();
    Task<IEnumerable<AcknowledgementRow>> GetAcknowledgementsByPaymentIdsAsync(IEnumerable<string> paymentIds);

    Task<IEnumerable<IncomeReceiptVoucherRow>> GetIncomeReceiptsAsync();
    Task<AcknowledgementAttachment?> GetAcknowledgementAttachmentAsync(string paymentId);
}

public class VoucherService : IVoucherService
{
    private readonly IDbConnectionFactory _db;

    public VoucherService(IDbConnectionFactory db)
    {
        _db = db;
    }

    // ------------------------------------------------------------------
    // Approvals
    // ------------------------------------------------------------------

    public async Task<IEnumerable<PendingApproval>> GetPendingApprovalsAsync(string role, string officialId)
    {
        using var conn = _db.CreateConnection();

        // Vouchers this specific official hasn't yet ruled on, restricted
        // to the role column that matches their seat (so a Chairman who
        // is also, say, a Trustee never sees vouchers meant only for the
        // Chapter Secretary's queue). Legacy vouchers are excluded
        // entirely - they predate this workflow by definition (that's
        // what Is_Legacy means) and were never meant to pass through an
        // approval queue in the first place.
        const string sql = @"
            SELECT
                v.Voucher_ID, v.VoucherDate, v.Transaction_Type, v.Amount,
                v.[Description],
                COALESCE(o.FullName, s.Business_Name, v.Manual_Payee_Name) AS PayeeDisplay,
                vas.ChairmanApproved, vas.ChapterSecretaryApproved,
                vas.ChairmanRejected, vas.ChapterSecretaryRejected,
                v.Supersedes_Voucher_ID,
                (SELECT TOP 1 va2.Comments
                 FROM Voucher_Approvals va2
                 WHERE va2.Voucher_ID = v.Supersedes_Voucher_ID AND va2.Approval_Status = 'Rejected'
                 ORDER BY va2.Approval_Date DESC) AS SupersededRejectionReason,
                bc.Budget_ID, bc.Category_Name AS Budget_Category,
                o.Role AS PayeeRole,
                CASE WHEN v.Official_Link = @OfficialID THEN 1 ELSE 0 END AS IsOwnVoucher,
                -- Monday of the week VoucherDate falls in, computed via
                -- the classic day-zero-was-a-Monday trick so it's
                -- independent of the server's @@DATEFIRST/locale setting
                -- - the grouping key for bulk approval.
                DATEADD(DAY, DATEDIFF(DAY, 0, v.VoucherDate) / 7 * 7, 0) AS WeekStart
            FROM Vouchers v
            LEFT JOIN Ref_Officials o ON o.OfficialID = v.Official_Link
            LEFT JOIN Ref_Suppliers s ON s.Supplier_ID = v.Supplier_Link
            LEFT JOIN VoucherApprovalStatus vas ON vas.Voucher_ID = v.Voucher_ID
            LEFT JOIN Ref_BudgetCodes bc ON bc.Budget_ID = v.Budget_Link
            WHERE v.Is_Legacy = 0
              -- Income is recorded directly (VoucherPaymentEligible treats
              -- every Income voucher as payment-eligible regardless of
              -- approval - see SQL/021) rather than routed through
              -- Chairman/Chapter Secretary approval, since most income
              -- arrives as a bank deposit, M-Pesa, or cash that's already
              -- landed by the time anyone types the voucher in.
              AND v.Transaction_Type <> 'Income'
              AND NOT EXISTS (
                SELECT 1 FROM Voucher_Approvals va
                WHERE va.Voucher_ID = v.Voucher_ID AND va.Approver_ID = @OfficialID
            )
              -- A rejection from EITHER role is final (trg_Voucher_
              -- Approvals_Validate now blocks an Approved row after one
              -- exists) - so once rejected, pull it from the OTHER
              -- approver's queue too rather than leaving something
              -- already dead sitting there to vote on.
              AND NOT EXISTS (
                SELECT 1 FROM Voucher_Approvals va2
                WHERE va2.Voucher_ID = v.Voucher_ID AND va2.Approval_Status = 'Rejected'
            )
            ORDER BY v.VoucherDate;";

        return await conn.QueryAsync<PendingApproval>(sql, new { OfficialID = officialId });
    }

    public async Task SubmitApprovalAsync(string voucherId, string officialId, string status, string? comments)
    {
        using var conn = _db.CreateConnection();

        // trg_Voucher_Approvals_Validate on the server still enforces
        // role and no-duplicate-approval rules; this insert can fail
        // with a SqlException that the calling page should catch and
        // show back to the user.
        const string sql = @"
            INSERT INTO Voucher_Approvals (Voucher_ID, Approver_ID, Approval_Status, Comments)
            VALUES (@VoucherId, @OfficialId, @Status, @Comments);";

        await conn.ExecuteAsync(sql, new { VoucherId = voucherId, OfficialId = officialId, Status = status, Comments = comments });
    }

    // One INSERT per voucher, not a single multi-row statement -
    // trg_Voucher_Approvals_Validate then evaluates each exactly as it
    // would for an individual Approve click, and a voucher that fails
    // (already ruled on by someone else in the moment between page load
    // and submit, say) is reported back without stopping the rest of
    // the group from going through.
    public async Task<IEnumerable<BulkApprovalResult>> BulkApproveAsync(IEnumerable<string> voucherIds, string officialId, string? comments)
    {
        using var conn = _db.CreateConnection();
        var results = new List<BulkApprovalResult>();

        const string sql = @"
            INSERT INTO Voucher_Approvals (Voucher_ID, Approver_ID, Approval_Status, Comments)
            VALUES (@VoucherId, @OfficialId, 'Approved', @Comments);";

        foreach (var voucherId in voucherIds)
        {
            try
            {
                await conn.ExecuteAsync(sql, new { VoucherId = voucherId, OfficialId = officialId, Comments = comments });
                results.Add(new BulkApprovalResult { Voucher_ID = voucherId, Success = true });
            }
            catch (SqlException ex)
            {
                results.Add(new BulkApprovalResult { Voucher_ID = voucherId, Success = false, Message = ex.Message });
            }
        }

        return results;
    }

    // ------------------------------------------------------------------
    // Voucher creation (dropdown data + insert)
    // ------------------------------------------------------------------

    public async Task<IEnumerable<SimpleOption>> GetBudgetCodesAsync()
    {
        using var conn = _db.CreateConnection();
        const string sql = "SELECT Budget_ID AS Id, Category_Name + ' (' + Category_Type + ')' AS Label FROM Ref_BudgetCodes ORDER BY Category_Name;";
        return await conn.QueryAsync<SimpleOption>(sql);
    }

    public async Task<IEnumerable<OfficialOption>> GetOfficialsAsync()
    {
        using var conn = _db.CreateConnection();
        const string sql = "SELECT OfficialID AS Id, FullName + ' (' + Role + ')' AS Label, FullName FROM Ref_Officials WHERE IsCurrent = 1 ORDER BY FullName;";
        return await conn.QueryAsync<OfficialOption>(sql);
    }

    public async Task<IEnumerable<SimpleOption>> GetSuppliersAsync()
    {
        using var conn = _db.CreateConnection();
        const string sql = "SELECT Supplier_ID AS Id, Business_Name AS Label FROM Ref_Suppliers ORDER BY Business_Name;";
        return await conn.QueryAsync<SimpleOption>(sql);
    }

    public async Task<IEnumerable<RejectedVoucherRow>> GetRejectedVouchersAwaitingCorrectionAsync()
    {
        using var conn = _db.CreateConnection();

        // A voucher can end up with more than one Rejected row (both
        // approvers could each reject it independently), so this is one
        // row per rejection event, not per voucher - informative rather
        // than a bug. "Awaiting correction" means no other voucher has
        // already superseded it yet.
        const string sql = @"
            SELECT
                v.Voucher_ID, v.VoucherDate, v.Amount, v.[Description],
                COALESCE(o.FullName, s.Business_Name, v.Manual_Payee_Name) AS PayeeDisplay,
                ro.Role AS RejectedByRole, va.Comments AS RejectionReason, va.Approval_Date AS RejectionDate
            FROM Vouchers v
            LEFT JOIN Ref_Officials o ON o.OfficialID = v.Official_Link
            LEFT JOIN Ref_Suppliers s ON s.Supplier_ID = v.Supplier_Link
            INNER JOIN Voucher_Approvals va ON va.Voucher_ID = v.Voucher_ID AND va.Approval_Status = 'Rejected'
            INNER JOIN Ref_Officials ro ON ro.OfficialID = va.Approver_ID
            WHERE v.Is_Legacy = 0
              AND NOT EXISTS (SELECT 1 FROM Vouchers v2 WHERE v2.Supersedes_Voucher_ID = v.Voucher_ID)
            ORDER BY va.Approval_Date DESC;";

        return await conn.QueryAsync<RejectedVoucherRow>(sql);
    }

    public async Task<string> CreateVoucherAsync(VoucherInputModel input)
    {
        using var conn = _db.CreateConnection();

        // If this voucher pays toward an existing debt, the debt's own
        // creditor is authoritative - override whatever the payee
        // fields say rather than trusting them to already agree. A
        // debt always represents money the union owes, so it's always
        // an Expense regardless of what the form's dropdown had.
        if (!string.IsNullOrWhiteSpace(input.Debt_Link))
        {
            const string debtSql = @"
                SELECT Creditor_Type, Official_link AS Official_Link, Supplier_link AS Supplier_Link
                FROM Debt_Register WHERE Debt_ID = @DebtId;";

            var debt = await conn.QuerySingleOrDefaultAsync(debtSql, new { DebtId = input.Debt_Link });

            if (debt is not null)
            {
                input.Transaction_Type = "Expense";
                input.Payee_Category = (string)debt.Creditor_Type;
                input.Official_Link = debt.Official_Link;
                input.Supplier_Link = debt.Supplier_Link;
                input.Manual_Payee_Name = null;
                // The debt's own creditor now has a reference-row
                // address (Ref_Officials/Ref_Suppliers) instead - a
                // one-time address typed against this voucher would be
                // stale the next time the same debt is paid down.
                input.Manual_Payee_Address = null;
            }
        }

        // Voucher_ID is server-generated (DEFAULT expression using
        // VoucherSeq), so OUTPUT INSERTED.Voucher_ID hands the new
        // number straight back instead of re-querying for it.
        const string sql = @"
            INSERT INTO Vouchers
                (VoucherDate, Transaction_Type, Budget_Link, Official_Link, Supplier_Link,
                 Amount, [Description], Manual_Payee_Name, Manual_Payee_Address, Payee_Category, Debt_link,
                 Is_Travel_Expense, Traveler_Name, Travel_From, Travel_To, Travel_Mode, Travel_Date, Travel_Reason,
                 Supersedes_Voucher_ID)
            OUTPUT INSERTED.Voucher_ID
            VALUES
                (@VoucherDate, @Transaction_Type, @Budget_Link, @Official_Link, @Supplier_Link,
                 @Amount, @Description, @Manual_Payee_Name, @Manual_Payee_Address, @Payee_Category, @Debt_Link,
                 @Is_Travel_Expense, @Traveler_Name, @Travel_From, @Travel_To, @Travel_Mode, @Travel_Date, @Travel_Reason,
                 @Supersedes_Voucher_ID);";

        var voucherId = await conn.ExecuteScalarAsync<string>(sql, input);

        // Income recorded as already received (the normal case - money's
        // usually already in the account, M-Pesa, or cash by the time
        // anyone gets around to entering the voucher) - record the
        // matching Payment/PaymentAllocations right away instead of
        // making the Treasurer make a separate trip to Record Payment.
        // VoucherPaymentEligible (SQL/021) already treats every Income
        // voucher as payment-eligible, so trg_PaymentAllocations_
        // RequireApproval allows this the moment the Vouchers insert
        // above has committed. Payment_Date uses the voucher's own date
        // (which may be backdated), not GETDATE(), so the payment
        // reflects when the income actually landed.
        if (input.Transaction_Type == "Income" && input.Mark_As_Received)
        {
            const string insertPayment = @"
                INSERT INTO Payments (Payment_Date, Payment_Mode, [Description], Amount_Paid, Reference_No, Bank_Account)
                OUTPUT INSERTED.Payment_ID
                VALUES (@VoucherDate, @Payment_Mode, @Description, @Amount, @Payment_Reference_No, @Payment_Bank_Account);";

            var paymentId = await conn.ExecuteScalarAsync<string>(insertPayment, input);

            const string insertAllocation = @"
                INSERT INTO PaymentAllocations (Payment_ID, Voucher_ID, Allocated_Amount)
                VALUES (@PaymentId, @VoucherId, @Amount);";

            await conn.ExecuteAsync(insertAllocation, new { PaymentId = paymentId, VoucherId = voucherId, Amount = input.Amount });
        }

        return voucherId!;
    }

    // Every voucher still awaiting a final decision - not fully approved,
    // not legacy (which was never meant to pass through approval at
    // all), and not already rejected (a rejected voucher is corrected by
    // creating a new one that supersedes it, per GetRejectedVouchers
    // AwaitingCorrectionAsync above, not by editing this one). CanEdit
    // on the row is computed from the two approval flags - true only
    // when neither role has recorded any decision yet.
    public async Task<IEnumerable<PendingVoucherRow>> GetPendingVouchersAsync()
    {
        using var conn = _db.CreateConnection();

        const string sql = @"
            SELECT v.Voucher_ID, v.VoucherDate, v.Transaction_Type, v.Amount, v.[Description],
                   COALESCE(o.FullName, s.Business_Name, v.Manual_Payee_Name) AS PayeeDisplay,
                   v.Payee_Category, v.Official_Link, v.Supplier_Link, v.Manual_Payee_Name, v.Manual_Payee_Address,
                   v.Budget_Link, v.Debt_link AS Debt_Link,
                   v.Is_Travel_Expense, v.Traveler_Name, v.Travel_From, v.Travel_To, v.Travel_Mode, v.Travel_Date, v.Travel_Reason,
                   vas.ChairmanApproved, vas.ChapterSecretaryApproved
            FROM Vouchers v
            INNER JOIN VoucherApprovalStatus vas ON vas.Voucher_ID = v.Voucher_ID
            LEFT JOIN Ref_Officials o ON o.OfficialID = v.Official_Link
            LEFT JOIN Ref_Suppliers s ON s.Supplier_ID = v.Supplier_Link
            WHERE v.Is_Legacy = 0
              AND vas.FullyApproved = 0
              AND NOT EXISTS (
                SELECT 1 FROM Voucher_Approvals va
                WHERE va.Voucher_ID = v.Voucher_ID AND va.Approval_Status = 'Rejected'
              )
              -- Auto-paid Income vouchers never pick up a Voucher_Approvals
              -- row (they skip approval entirely) and so never trip
              -- FullyApproved - without this they'd sit here as
              -- 'pending' forever even once paid.
              AND NOT EXISTS (
                SELECT 1 FROM PaymentAllocations pa
                WHERE pa.Voucher_ID = v.Voucher_ID
              )
            ORDER BY v.VoucherDate;";

        return await conn.QueryAsync<PendingVoucherRow>(sql);
    }

    public async Task UpdateVoucherAsync(VoucherInputModel input, string voucherId)
    {
        using var conn = _db.CreateConnection();

        // Same debt-authoritative override as CreateVoucherAsync - if
        // this voucher was (or is being) linked to a debt, the debt's
        // creditor wins over whatever the form posted.
        if (!string.IsNullOrWhiteSpace(input.Debt_Link))
        {
            const string debtSql = @"
                SELECT Creditor_Type, Official_link AS Official_Link, Supplier_link AS Supplier_Link
                FROM Debt_Register WHERE Debt_ID = @DebtId;";

            var debt = await conn.QuerySingleOrDefaultAsync(debtSql, new { DebtId = input.Debt_Link });

            if (debt is not null)
            {
                input.Transaction_Type = "Expense";
                input.Payee_Category = (string)debt.Creditor_Type;
                input.Official_Link = debt.Official_Link;
                input.Supplier_Link = debt.Supplier_Link;
                input.Manual_Payee_Name = null;
                input.Manual_Payee_Address = null;
            }
        }

        // trg_Vouchers_BlockEditAfterApproval (SQL/020) rejects this
        // outright if any Voucher_Approvals row already exists for this
        // voucher - Pending.cshtml only offers Edit while CanEdit is
        // true, but the trigger is what actually guarantees an
        // approver's decision can never be undermined by a field
        // changing out from under it after the fact.
        const string sql = @"
            UPDATE Vouchers SET
                VoucherDate = @VoucherDate,
                Transaction_Type = @Transaction_Type,
                Budget_Link = @Budget_Link,
                Official_Link = @Official_Link,
                Supplier_Link = @Supplier_Link,
                Amount = @Amount,
                [Description] = @Description,
                Manual_Payee_Name = @Manual_Payee_Name,
                Manual_Payee_Address = @Manual_Payee_Address,
                Payee_Category = @Payee_Category,
                Debt_link = @Debt_Link,
                Is_Travel_Expense = @Is_Travel_Expense,
                Traveler_Name = @Traveler_Name,
                Travel_From = @Travel_From,
                Travel_To = @Travel_To,
                Travel_Mode = @Travel_Mode,
                Travel_Date = @Travel_Date,
                Travel_Reason = @Travel_Reason
            WHERE Voucher_ID = @VoucherId;";

        var parameters = new DynamicParameters(input);
        parameters.Add("VoucherId", voucherId);
        await conn.ExecuteAsync(sql, parameters);
    }

    // Every voucher that has cleared approval (FullyApproved) or predates
    // the workflow entirely (Is_Legacy), within one reporting period -
    // the caller computes [start, end) from whichever Year/Quarter/
    // HalfYear/Month tab is selected, so this stays a single simple
    // range filter regardless of which granularity the page is showing.
    public async Task<IEnumerable<ApprovedVoucherRow>> GetApprovedVouchersAsync(DateTime start, DateTime end)
    {
        using var conn = _db.CreateConnection();

        const string sql = @"
            SELECT v.Voucher_ID, v.VoucherDate, v.Transaction_Type, v.Amount, v.[Description],
                   COALESCE(o.FullName, s.Business_Name, v.Manual_Payee_Name) AS PayeeDisplay,
                   v.Payee_Category, bc.Category_Name AS Budget_Category,
                   v.Is_Travel_Expense, v.Traveler_Name, v.Travel_From, v.Travel_To, v.Travel_Mode, v.Travel_Date, v.Travel_Reason,
                   v.Is_Legacy, v.Current_Status
            FROM Vouchers v
            INNER JOIN VoucherApprovalStatus vas ON vas.Voucher_ID = v.Voucher_ID
            LEFT JOIN Ref_Officials o ON o.OfficialID = v.Official_Link
            LEFT JOIN Ref_Suppliers s ON s.Supplier_ID = v.Supplier_Link
            LEFT JOIN Ref_BudgetCodes bc ON bc.Budget_ID = v.Budget_Link
            WHERE (v.Is_Legacy = 1 OR vas.FullyApproved = 1 OR v.Transaction_Type = 'Income')
              AND v.VoucherDate >= @Start AND v.VoucherDate < @End
            ORDER BY v.VoucherDate DESC;";

        return await conn.QueryAsync<ApprovedVoucherRow>(sql, new { Start = start, End = end });
    }

    public async Task<IEnumerable<VoucherApprovalActionRow>> GetVoucherApprovalActionsAsync(DateTime start, DateTime end)
    {
        using var conn = _db.CreateConnection();

        const string sql = @"
            SELECT va.Voucher_ID, o.Role AS ApproverRole, o.FullName AS ApproverName,
                   va.Approval_Status, va.Approval_Date, va.Comments
            FROM Voucher_Approvals va
            INNER JOIN Vouchers v ON v.Voucher_ID = va.Voucher_ID
            INNER JOIN Ref_Officials o ON o.OfficialID = va.Approver_ID
            WHERE v.VoucherDate >= @Start AND v.VoucherDate < @End
            ORDER BY va.Voucher_ID, va.Approval_Date;";

        return await conn.QueryAsync<VoucherApprovalActionRow>(sql, new { Start = start, End = end });
    }

    // Same shape as GetVoucherApprovalActionsAsync, filtered by specific
    // Voucher_IDs rather than a date range - for Form R, where the
    // vouchers behind one withdrawal can have any VoucherDate.
    public async Task<IEnumerable<VoucherApprovalActionRow>> GetVoucherApprovalActionsByIdsAsync(IEnumerable<string> voucherIds)
    {
        using var conn = _db.CreateConnection();

        const string sql = @"
            SELECT va.Voucher_ID, o.Role AS ApproverRole, o.FullName AS ApproverName,
                   va.Approval_Status, va.Approval_Date, va.Comments
            FROM Voucher_Approvals va
            INNER JOIN Ref_Officials o ON o.OfficialID = va.Approver_ID
            WHERE va.Voucher_ID IN @VoucherIds
            ORDER BY va.Voucher_ID, va.Approval_Date;";

        return await conn.QueryAsync<VoucherApprovalActionRow>(sql, new { VoucherIds = voucherIds });
    }

    public async Task<IEnumerable<VoucherPaymentRow>> GetVoucherPaymentsAsync(DateTime start, DateTime end)
    {
        using var conn = _db.CreateConnection();

        const string sql = @"
            SELECT pa.Voucher_ID, p.Payment_ID, p.Payment_Date, p.Payment_Mode, pa.Allocated_Amount
            FROM PaymentAllocations pa
            INNER JOIN Payments p ON p.Payment_ID = pa.Payment_ID
            INNER JOIN Vouchers v ON v.Voucher_ID = pa.Voucher_ID
            WHERE v.VoucherDate >= @Start AND v.VoucherDate < @End
            ORDER BY pa.Voucher_ID, p.Payment_Date;";

        return await conn.QueryAsync<VoucherPaymentRow>(sql, new { Start = start, End = end });
    }

    // ------------------------------------------------------------------
    // Payments
    // ------------------------------------------------------------------

    public async Task<IEnumerable<SimpleOption>> GetFullyApprovedUnpaidVouchersAsync()
    {
        using var conn = _db.CreateConnection();

        // VoucherPaymentEligible covers both cases trg_PaymentAllocations_
        // RequireApproval actually allows: fully approved, or explicitly
        // marked Is_Legacy (paid before this workflow existed). Reading
        // from the same view the trigger checks means this dropdown and
        // the trigger can never drift out of sync with each other.
        const string sql = @"
            SELECT v.Voucher_ID AS Id,
                   v.Voucher_ID + ' - ' + FORMAT(v.Amount,'N2') + ' - ' + ISNULL(v.[Description],'')
                   + CASE WHEN v.Is_Legacy = 1 THEN ' (legacy)' ELSE '' END AS Label
            FROM Vouchers v
            INNER JOIN VoucherPaymentEligible vpe ON vpe.Voucher_ID = v.Voucher_ID
            WHERE v.Current_Status <> 'Paid'
            ORDER BY v.VoucherDate;";

        return await conn.QueryAsync<SimpleOption>(sql);
    }

    public async Task<IEnumerable<ApprovedUnpaidVoucherRow>> GetApprovedUnpaidVouchersDetailedAsync()
    {
        using var conn = _db.CreateConnection();

        // Same source as GetFullyApprovedUnpaidVouchersAsync
        // (VoucherPaymentEligible, kept in sync with trg_PaymentAllocations_
        // RequireApproval) - just the full row instead of a terse dropdown
        // label, for the browsable "what's ready to pay" view.
        const string sql = @"
            SELECT
                v.Voucher_ID, v.VoucherDate, v.Transaction_Type, v.Amount, v.[Description],
                COALESCE(o.FullName, s.Business_Name, v.Manual_Payee_Name) AS PayeeDisplay,
                v.Is_Legacy
            FROM Vouchers v
            INNER JOIN VoucherPaymentEligible vpe ON vpe.Voucher_ID = v.Voucher_ID
            LEFT JOIN Ref_Officials o ON o.OfficialID = v.Official_Link
            LEFT JOIN Ref_Suppliers s ON s.Supplier_ID = v.Supplier_Link
            WHERE v.Current_Status <> 'Paid'
            ORDER BY v.VoucherDate;";

        return await conn.QueryAsync<ApprovedUnpaidVoucherRow>(sql);
    }

    // Settles one or more approved vouchers together with a single
    // payment - each voucher in Voucher_IDs is allocated in full (its
    // own Amount, looked up here rather than trusted from the form),
    // so Amount_Paid is expected to equal their sum. Everything runs in
    // one real SQL transaction: previously this method's three-ish
    // inserts had no explicit transaction at all (a documented gap -
    // a failure partway through could leave a payment with only some
    // of its intended allocations, or no withdrawal link). That risk
    // is much more real now that a single submission can span several
    // PaymentAllocations inserts instead of just one.
    public async Task<string> RecordPaymentAsync(PaymentInputModel input)
    {
        using var conn = (SqlConnection)_db.CreateConnection();
        conn.Open();
        using var transaction = conn.BeginTransaction();

        try
        {
            const string insertPayment = @"
                INSERT INTO Payments (Payment_Date, Payment_Mode, [Description], Amount_Paid, Reference_No, Bank_Account)
                OUTPUT INSERTED.Payment_ID
                VALUES (GETDATE(), @Payment_Mode, @Description, @Amount_Paid, @Reference_No, @Bank_Account);";

            var paymentId = await conn.ExecuteScalarAsync<string>(insertPayment, input, transaction);

            // trg_PaymentAllocations_RequireApproval and trg_PaymentStatusUpdate
            // fire per-insert here, same as they would for a single-voucher
            // payment - an unapproved voucher, or an amount exceeding what a
            // voucher is actually worth, rolls back the whole transaction.
            const string voucherAmountSql = "SELECT Amount FROM Vouchers WHERE Voucher_ID = @VoucherId;";
            const string insertAllocation = @"
                INSERT INTO PaymentAllocations (Payment_ID, Voucher_ID, Allocated_Amount)
                VALUES (@PaymentId, @VoucherId, @Amount);";

            foreach (var voucherId in input.Voucher_IDs)
            {
                var voucherAmount = await conn.ExecuteScalarAsync<decimal>(voucherAmountSql, new { VoucherId = voucherId }, transaction);
                await conn.ExecuteAsync(insertAllocation,
                    new { PaymentId = paymentId, VoucherId = voucherId, Amount = voucherAmount }, transaction);
            }

            // Optional: if this payment was funded from cash drawn via a
            // specific withdrawal, link the two.
            if (!string.IsNullOrWhiteSpace(input.Withdrawal_Link))
            {
                const string insertWithdrawalLink = @"
                    INSERT INTO WithdrawalPayments (Withdrawal_ID, Payment_ID, Allocated_Amount)
                    VALUES (@WithdrawalId, @PaymentId, @Amount);";

                await conn.ExecuteAsync(insertWithdrawalLink,
                    new { WithdrawalId = input.Withdrawal_Link, PaymentId = paymentId, Amount = input.Amount_Paid },
                    transaction);
            }

            transaction.Commit();
            return paymentId!;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    // ------------------------------------------------------------------
    // Acknowledgements
    // ------------------------------------------------------------------

    public async Task<IEnumerable<PaymentAcknowledgementOption>> GetPaymentsAwaitingAcknowledgementAsync()
    {
        using var conn = _db.CreateConnection();

        // Same payee-resolution logic as GetPendingApprovalsAsync (Official
        // name, else Supplier business name, else the manual one-time
        // name), so the "Acknowledged By" field can be prefilled with
        // whichever one actually applies - avoids re-typing (and
        // mistyping) a name the app already knows. Left NULL when a
        // payment's allocations span more than one distinct payee, since
        // there's no single right answer to prefill in that case.
        const string sql = @"
            SELECT
                p.Payment_ID AS Id,
                p.Payment_ID + ' - ' + FORMAT(p.Amount_Paid,'N2') + ' - ' + p.Payment_Mode AS Label,
                CASE WHEN COUNT(DISTINCT COALESCE(o.FullName, s.Business_Name, v.Manual_Payee_Name)) = 1
                     THEN MIN(COALESCE(o.FullName, s.Business_Name, v.Manual_Payee_Name))
                     ELSE NULL END AS PayeeName
            FROM PaymentsAwaitingAcknowledgement p
            INNER JOIN PaymentAllocations al ON al.Payment_ID = p.Payment_ID
            INNER JOIN Vouchers v ON v.Voucher_ID = al.Voucher_ID
            LEFT JOIN Ref_Officials o ON o.OfficialID = v.Official_Link
            LEFT JOIN Ref_Suppliers s ON s.Supplier_ID = v.Supplier_Link
            GROUP BY p.Payment_ID, p.Payment_Date, p.Amount_Paid, p.Payment_Mode
            ORDER BY p.Payment_Date;";
        return await conn.QueryAsync<PaymentAcknowledgementOption>(sql);
    }

    public async Task RecordAcknowledgementAsync(AcknowledgementInputModel input)
    {
        using var conn = _db.CreateConnection();

        byte[]? attachmentData = null;
        string? attachmentFileName = null;
        string? attachmentContentType = null;

        if (input.Attachment is { Length: > 0 } file)
        {
            if (file.Length > MaxAttachmentBytes)
                throw new InvalidOperationException("Attachment must be 5MB or smaller.");

            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            attachmentData = ms.ToArray();

            // Sniff the actual bytes rather than trusting the browser-
            // supplied Content-Type - this is a permanent write into a
            // financial record, not just a display hint.
            if (!TryDetectImageContentType(attachmentData, out attachmentContentType))
                throw new InvalidOperationException("Attachment must be a JPEG or PNG image.");

            attachmentFileName = Path.GetFileName(file.FileName);
        }

        const string sql = @"
            INSERT INTO Payment_Acknowledgements
                (Payment_ID, Acknowledged_By, Method, Notes, Attachment_Data, Attachment_FileName, Attachment_ContentType)
            VALUES
                (@Payment_ID, @Acknowledged_By, @Method, @Notes, @AttachmentData, @AttachmentFileName, @AttachmentContentType);";

        await conn.ExecuteAsync(sql, new
        {
            input.Payment_ID,
            input.Acknowledged_By,
            input.Method,
            input.Notes,
            AttachmentData = attachmentData,
            AttachmentFileName = attachmentFileName,
            AttachmentContentType = attachmentContentType
        });
    }

    public async Task<IEnumerable<AcknowledgementRow>> GetRecentAcknowledgementsAsync()
    {
        using var conn = _db.CreateConnection();
        const string sql = @"
            SELECT TOP 5
                Payment_ID, Acknowledgement_Date, Acknowledged_By, Method, Notes,
                CASE WHEN Attachment_Data IS NOT NULL THEN 1 ELSE 0 END AS HasAttachment
            FROM Payment_Acknowledgements
            ORDER BY Acknowledgement_Date DESC;";
        return await conn.QueryAsync<AcknowledgementRow>(sql);
    }

    public async Task<IEnumerable<AcknowledgementRow>> GetAllAcknowledgementsAsync()
    {
        using var conn = _db.CreateConnection();
        const string sql = @"
            SELECT
                Payment_ID, Acknowledgement_Date, Acknowledged_By, Method, Notes,
                CASE WHEN Attachment_Data IS NOT NULL THEN 1 ELSE 0 END AS HasAttachment
            FROM Payment_Acknowledgements
            ORDER BY Acknowledgement_Date DESC;";
        return await conn.QueryAsync<AcknowledgementRow>(sql);
    }

    // Same shape as GetAllAcknowledgementsAsync, filtered to specific
    // Payment_IDs - for Form R, where a withdrawal can fund more than
    // one payment (and Payment_Acknowledgements is keyed by Payment_ID,
    // not by withdrawal).
    public async Task<IEnumerable<AcknowledgementRow>> GetAcknowledgementsByPaymentIdsAsync(IEnumerable<string> paymentIds)
    {
        using var conn = _db.CreateConnection();
        const string sql = @"
            SELECT
                Payment_ID, Acknowledgement_Date, Acknowledged_By, Method, Notes,
                CASE WHEN Attachment_Data IS NOT NULL THEN 1 ELSE 0 END AS HasAttachment
            FROM Payment_Acknowledgements
            WHERE Payment_ID IN @PaymentIds
            ORDER BY Acknowledgement_Date DESC;";
        return await conn.QueryAsync<AcknowledgementRow>(sql, new { PaymentIds = paymentIds });
    }

    public async Task<AcknowledgementAttachment?> GetAcknowledgementAttachmentAsync(string paymentId)
    {
        using var conn = _db.CreateConnection();
        const string sql = @"
            SELECT Attachment_Data, Attachment_FileName, Attachment_ContentType
            FROM Payment_Acknowledgements
            WHERE Payment_ID = @PaymentId AND Attachment_Data IS NOT NULL;";
        return await conn.QuerySingleOrDefaultAsync<AcknowledgementAttachment>(sql, new { PaymentId = paymentId });
    }

    // Every Income voucher/payment, flattened to (payment, voucher)
    // pairs - FormQModel groups these by Payment_ID and filters to
    // payments actually received from the National Office (PayerName
    // containing "National"), same reasoning as the payee resolution
    // used everywhere else: COALESCE(Official, Supplier, manual name).
    public async Task<IEnumerable<IncomeReceiptVoucherRow>> GetIncomeReceiptsAsync()
    {
        using var conn = _db.CreateConnection();

        const string sql = @"
            SELECT p.Payment_ID, p.Payment_Date, p.Payment_Mode, p.Reference_No, p.Bank_Account,
                   COALESCE(o.FullName, s.Business_Name, v.Manual_Payee_Name) AS PayerName,
                   v.Voucher_ID, bc.Category_Name,
                   pa.Allocated_Amount AS AllocatedFromThisPayment
            FROM Payments p
            INNER JOIN PaymentAllocations pa ON pa.Payment_ID = p.Payment_ID
            INNER JOIN Vouchers v ON v.Voucher_ID = pa.Voucher_ID
            LEFT JOIN Ref_Officials o ON o.OfficialID = v.Official_Link
            LEFT JOIN Ref_Suppliers s ON s.Supplier_ID = v.Supplier_Link
            LEFT JOIN Ref_BudgetCodes bc ON bc.Budget_ID = v.Budget_Link
            WHERE v.Transaction_Type = 'Income'
            ORDER BY p.Payment_Date DESC, p.Payment_ID;";

        return await conn.QueryAsync<IncomeReceiptVoucherRow>(sql);
    }

    private const long MaxAttachmentBytes = 5 * 1024 * 1024;

    // Checked against the file's actual bytes, not its extension or
    // declared Content-Type, both of which the uploader fully controls.
    private static bool TryDetectImageContentType(byte[] data, out string? contentType)
    {
        if (data.Length >= 3 && data[0] == 0xFF && data[1] == 0xD8 && data[2] == 0xFF)
        {
            contentType = "image/jpeg";
            return true;
        }

        if (data.Length >= 8 &&
            data[0] == 0x89 && data[1] == 0x50 && data[2] == 0x4E && data[3] == 0x47 &&
            data[4] == 0x0D && data[5] == 0x0A && data[6] == 0x1A && data[7] == 0x0A)
        {
            contentType = "image/png";
            return true;
        }

        contentType = null;
        return false;
    }
}
