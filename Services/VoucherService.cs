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
    Task<IReadOnlyList<PossibleDuplicateRow>> FindPossibleDuplicatesAsync(VoucherInputModel input);

    Task<IEnumerable<PendingVoucherRow>> GetPendingVouchersAsync();
    Task UpdateVoucherAsync(VoucherInputModel input, string voucherId);
    Task VoidVoucherAsync(string voucherId, string officialId, string reason);

    Task<IEnumerable<ApprovedVoucherRow>> GetApprovedVouchersAsync(DateTime start, DateTime end);
    Task<IEnumerable<VoucherApprovalActionRow>> GetVoucherApprovalActionsAsync(DateTime start, DateTime end);
    Task<IEnumerable<VoucherApprovalActionRow>> GetVoucherApprovalActionsByIdsAsync(IEnumerable<string> voucherIds);
    Task<IEnumerable<VoucherPaymentRow>> GetVoucherPaymentsAsync(DateTime start, DateTime end);

    Task<IEnumerable<SimpleOption>> GetFullyApprovedUnpaidVouchersAsync();
    Task<IEnumerable<ApprovedUnpaidVoucherRow>> GetApprovedUnpaidVouchersDetailedAsync();
    Task<IReadOnlyList<RecordedPayment>> RecordPaymentAsync(PaymentInputModel input);

    Task<IEnumerable<PaymentAcknowledgementOption>> GetPaymentsAwaitingAcknowledgementAsync();
    Task RecordAcknowledgementAsync(AcknowledgementInputModel input);
    Task<IEnumerable<AcknowledgementBacklogRow>> GetAcknowledgementBacklogAsync();
    Task<int> RecordAcknowledgementsAsync(IReadOnlyList<AcknowledgementLineInput> lines);

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
              AND v.Voided_Date IS NULL
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
        // Former officials (IsCurrent = 0) are still valid payees - e.g.
        // arrears or refunds owed after leaving office - so list them too,
        // after the current officers and clearly tagged.
        const string sql = @"
            SELECT OfficialID AS Id,
                   FullName + ' (' + Role + CASE WHEN IsCurrent = 1 THEN '' ELSE ' - former' END + ')' AS Label,
                   FullName
            FROM Ref_Officials
            ORDER BY IsCurrent DESC, FullName;";
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
              -- A voided voucher (e.g. a rejected duplicate) needs no correction.
              AND v.Voided_Date IS NULL
              AND NOT EXISTS (SELECT 1 FROM Vouchers v2 WHERE v2.Supersedes_Voucher_ID = v.Voucher_ID)
            ORDER BY va.Approval_Date DESC;";

        return await conn.QueryAsync<RejectedVoucherRow>(sql);
    }

    // If this voucher pays toward an existing debt, the debt's own
    // creditor is authoritative - override whatever the payee fields say
    // rather than trusting them to already agree. A debt always
    // represents money the union owes, so it's always an Expense
    // regardless of what the form's dropdown had. Shared by create,
    // update and the duplicate check, so all three see the same payee.
    private static async Task ApplyDebtOverrideAsync(System.Data.IDbConnection conn, VoucherInputModel input)
    {
        if (string.IsNullOrWhiteSpace(input.Debt_Link))
            return;

        const string debtSql = @"
            SELECT Creditor_Type, Official_link AS Official_Link, Supplier_link AS Supplier_Link
            FROM Debt_Register WHERE Debt_ID = @DebtId;";

        var debt = await conn.QuerySingleOrDefaultAsync(debtSql, new { DebtId = input.Debt_Link });
        if (debt is null)
            return;

        input.Transaction_Type = "Expense";
        input.Payee_Category = (string)debt.Creditor_Type;
        input.Official_Link = debt.Official_Link;
        input.Supplier_Link = debt.Supplier_Link;
        input.Manual_Payee_Name = null;
        // The debt's own creditor now has a reference-row address
        // (Ref_Officials/Ref_Suppliers) instead - a one-time address typed
        // against this voucher would be stale the next time the same debt
        // is paid down.
        input.Manual_Payee_Address = null;
    }

    // Live (not rejected, not voided) vouchers that look like the one
    // about to be created: same transaction type and payee, and either
    // the same amount within 30 days either side, or the exact same
    // description. A warning only - the page lets the Treasurer confirm
    // and go ahead, since some payments genuinely repeat (monthly
    // honoraria for the same official, say).
    public async Task<IReadOnlyList<PossibleDuplicateRow>> FindPossibleDuplicatesAsync(VoucherInputModel input)
    {
        using var conn = _db.CreateConnection();
        await ApplyDebtOverrideAsync(conn, input);

        const string sql = @"
            SELECT TOP 5
                v.Voucher_ID, v.VoucherDate, v.Amount, v.[Description],
                COALESCE(o.FullName, s.Business_Name, v.Manual_Payee_Name) AS PayeeDisplay,
                CASE
                    WHEN v.Current_Status IN ('Paid', 'Partially Paid') THEN v.Current_Status
                    WHEN vas.FullyApproved = 1 THEN 'Approved - not paid'
                    ELSE 'Awaiting approval'
                END AS Status
            FROM Vouchers v
            LEFT JOIN VoucherApprovalStatus vas ON vas.Voucher_ID = v.Voucher_ID
            LEFT JOIN Ref_Officials o ON o.OfficialID = v.Official_Link
            LEFT JOIN Ref_Suppliers s ON s.Supplier_ID = v.Supplier_Link
            WHERE v.Voided_Date IS NULL
              AND NOT EXISTS (SELECT 1 FROM Voucher_Approvals va
                              WHERE va.Voucher_ID = v.Voucher_ID AND va.Approval_Status = 'Rejected')
              AND v.Transaction_Type = @Transaction_Type
              AND (
                    (@Official_Link IS NOT NULL AND v.Official_Link = @Official_Link)
                 OR (@Supplier_Link IS NOT NULL AND v.Supplier_Link = @Supplier_Link)
                 OR (@Manual_Payee_Name IS NOT NULL
                     AND UPPER(LTRIM(RTRIM(v.Manual_Payee_Name))) = UPPER(LTRIM(RTRIM(@Manual_Payee_Name))))
              )
              AND (
                    (v.Amount = @Amount AND ABS(DATEDIFF(DAY, v.VoucherDate, @VoucherDate)) <= 30)
                 OR (@Description IS NOT NULL AND LTRIM(RTRIM(@Description)) <> ''
                     AND LTRIM(RTRIM(v.[Description])) = LTRIM(RTRIM(@Description)))
              )
            ORDER BY ABS(DATEDIFF(DAY, v.VoucherDate, @VoucherDate)), v.Voucher_ID DESC;";

        // Only the payee field that matches the category counts - the
        // form posts all three regardless of which one is showing.
        var official = input.Payee_Category == "Official" ? input.Official_Link : null;
        var supplier = input.Payee_Category == "Supplier" ? input.Supplier_Link : null;
        var manual = input.Payee_Category is not ("Official" or "Supplier") && !string.IsNullOrWhiteSpace(input.Manual_Payee_Name)
            ? input.Manual_Payee_Name : null;

        return (await conn.QueryAsync<PossibleDuplicateRow>(sql, new
        {
            input.Transaction_Type,
            Official_Link = official,
            Supplier_Link = supplier,
            Manual_Payee_Name = manual,
            input.Amount,
            input.VoucherDate,
            input.Description
        })).ToList();
    }

    // Marks a voucher Voided (final - it keeps its number but can never be
    // approved, edited or paid). SQL/033's trigger and CHECK constraint
    // refuse a voucher that already has a payment, or retires cash float
    // receipts; the SqlException is surfaced by the calling page.
    public async Task VoidVoucherAsync(string voucherId, string officialId, string reason)
    {
        using var conn = _db.CreateConnection();

        const string sql = @"
            UPDATE Vouchers SET
                Current_Status = 'Voided',
                Voided_Date = GETDATE(),
                Voided_By = @OfficialId,
                Void_Reason = @Reason
            WHERE Voucher_ID = @VoucherId AND Voided_Date IS NULL;";

        var rows = await conn.ExecuteAsync(sql, new { VoucherId = voucherId, OfficialId = officialId, Reason = reason.Trim() });
        if (rows == 0)
            throw new InvalidOperationException($"Voucher {voucherId} was not found or is already voided.");
    }

    public async Task<string> CreateVoucherAsync(VoucherInputModel input)
    {
        using var conn = _db.CreateConnection();

        await ApplyDebtOverrideAsync(conn, input);

        // Voucher_ID is server-generated (DEFAULT expression using
        // VoucherSeq), so OUTPUT INSERTED.Voucher_ID hands the new
        // number straight back instead of re-querying for it. It goes
        // INTO a table variable because Vouchers has an INSERT trigger
        // (SQL/035), and SQL Server refuses a bare OUTPUT on such a table.
        const string sql = @"
            DECLARE @Inserted TABLE (Voucher_ID NVARCHAR(20));
            INSERT INTO Vouchers
                (VoucherDate, Transaction_Type, Budget_Link, Official_Link, Supplier_Link,
                 Amount, [Description], Manual_Payee_Name, Manual_Payee_Address, Payee_Category, Debt_link,
                 Is_Travel_Expense, Traveler_Name, Travel_From, Travel_To, Travel_Mode, Travel_Date, Travel_Reason,
                 Supersedes_Voucher_ID)
            OUTPUT INSERTED.Voucher_ID INTO @Inserted
            VALUES
                (@VoucherDate, @Transaction_Type, @Budget_Link, @Official_Link, @Supplier_Link,
                 @Amount, @Description, @Manual_Payee_Name, @Manual_Payee_Address, @Payee_Category, @Debt_Link,
                 @Is_Travel_Expense, @Traveler_Name, @Travel_From, @Travel_To, @Travel_Mode, @Travel_Date, @Travel_Reason,
                 @Supersedes_Voucher_ID);
            SELECT Voucher_ID FROM @Inserted;";

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
                   vas.ChairmanApproved, vas.ChapterSecretaryApproved,
                   CAST(CASE WHEN EXISTS (SELECT 1 FROM CashFloatReceipts r WHERE r.Voucher_ID = v.Voucher_ID)
                        THEN 1 ELSE 0 END AS bit) AS IsFloatVoucher
            FROM Vouchers v
            INNER JOIN VoucherApprovalStatus vas ON vas.Voucher_ID = v.Voucher_ID
            LEFT JOIN Ref_Officials o ON o.OfficialID = v.Official_Link
            LEFT JOIN Ref_Suppliers s ON s.Supplier_ID = v.Supplier_Link
            WHERE v.Is_Legacy = 0
              AND v.Voided_Date IS NULL
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
        await ApplyDebtOverrideAsync(conn, input);

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
              AND v.Voided_Date IS NULL
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
                v.Is_Legacy,
                ISNULL(paid.PaidSoFar, 0) AS PaidSoFar
            FROM Vouchers v
            INNER JOIN VoucherPaymentEligible vpe ON vpe.Voucher_ID = v.Voucher_ID
            LEFT JOIN Ref_Officials o ON o.OfficialID = v.Official_Link
            LEFT JOIN Ref_Suppliers s ON s.Supplier_ID = v.Supplier_Link
            OUTER APPLY (SELECT SUM(pa.Allocated_Amount) AS PaidSoFar
                         FROM PaymentAllocations pa WHERE pa.Voucher_ID = v.Voucher_ID) paid
            WHERE v.Current_Status <> 'Paid'
            ORDER BY v.VoucherDate;";

        return await conn.QueryAsync<ApprovedUnpaidVoucherRow>(sql);
    }

    // The date a payment is recorded under is what the cash book and Monthly
    // Certification use, so it has to be the day the money actually left -
    // within limits that keep the books consistent:
    //   - not in the future;
    //   - not before the withdrawal that funded it (that cash didn't exist yet);
    //   - not in, or before, the latest certified month - book balances are
    //     cumulative, so a payment dated there would change figures already
    //     certified.
    // Blank means today. Returns the date to store.
    private static async Task<DateTime> ValidatePaymentDateAsync(
        SqlConnection conn, SqlTransaction transaction, DateTime? requested, string? withdrawalId)
    {
        var date = (requested ?? DateTime.Today).Date;

        if (date > DateTime.Today)
            throw new InvalidOperationException($"The date paid ({date:d MMM yyyy}) is in the future.");

        if (!string.IsNullOrWhiteSpace(withdrawalId))
        {
            var withdrawn = await conn.ExecuteScalarAsync<DateTime?>(
                "SELECT Withdrawal_Date FROM BankWithdrawals WHERE Withdrawal_ID = @Id;",
                new { Id = withdrawalId }, transaction);
            if (withdrawn is not null && date < withdrawn.Value.Date)
                throw new InvalidOperationException(
                    $"The date paid ({date:d MMM yyyy}) is before {withdrawalId} was withdrawn ({withdrawn.Value:d MMM yyyy}). Cash from a withdrawal can't be paid out before it was drawn.");
        }

        var latest = await conn.QuerySingleOrDefaultAsync<(int Year, int Month)?>(@"
            SELECT TOP 1 Period_Year, Period_Month FROM MonthlyCertifications
            ORDER BY Period_Year DESC, Period_Month DESC;", transaction: transaction);
        if (latest is { } c && (date.Year < c.Year || (date.Year == c.Year && date.Month <= c.Month)))
        {
            var certified = new DateTime(c.Year, c.Month, 1);
            throw new InvalidOperationException(
                $"{certified:MMMM yyyy} has already been certified, so a payment can't be dated {date:d MMM yyyy} - it would change certified figures. " +
                $"Date it {certified.AddMonths(1):d MMM yyyy} or later, and explain the late entry in that month's certification notes.");
        }

        return date;
    }

    private class PayableVoucher
    {
        public string Voucher_ID { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal PaidSoFar { get; set; }
        public decimal PayNow { get; set; }
        public string PayeeKey { get; set; } = string.Empty;
        public string? PayeeDisplay { get; set; }
    }

    // Settles one or more approved vouchers with ONE PAYMENT PER PAYEE:
    // ticked vouchers for the same payee share a single payment (one
    // PaymentAllocations row per voucher, for the amount entered against
    // it, or if none its whole outstanding balance - worked out here from
    // earlier allocations, not trusted from the form). Paying less than
    // the balance is a part-payment: trg_PaymentStatusUpdate marks the
    // voucher 'Partially Paid', it stays payable for the rest, and the
    // trigger still refuses anything beyond its Amount. Vouchers for
    // different payees get separate payments. The mode, reference, bank
    // account, description and withdrawal on the form apply to every
    // payment. All of it runs in one SQL transaction: a voucher failing a
    // trigger rolls back every payment in the batch, so the Treasurer
    // never ends up with only some of the ticked vouchers paid.
    public async Task<IReadOnlyList<RecordedPayment>> RecordPaymentAsync(PaymentInputModel input)
    {
        using var conn = (SqlConnection)_db.CreateConnection();
        conn.Open();
        using var transaction = conn.BeginTransaction();

        try
        {
            // The payee key is the voucher's category plus whichever link
            // identifies the payee (official, supplier, or the typed name
            // for one-time payees - compared trimmed, case-insensitively).
            const string voucherSql = @"
                SELECT v.Voucher_ID, v.Amount,
                       ISNULL((SELECT SUM(pa.Allocated_Amount) FROM PaymentAllocations pa
                               WHERE pa.Voucher_ID = v.Voucher_ID), 0) AS PaidSoFar,
                       ISNULL(v.Payee_Category, '') + '|' + COALESCE(v.Official_Link, v.Supplier_Link,
                           UPPER(LTRIM(RTRIM(v.Manual_Payee_Name))), '') AS PayeeKey,
                       COALESCE(o.FullName, s.Business_Name, v.Manual_Payee_Name) AS PayeeDisplay
                FROM Vouchers v
                LEFT JOIN Ref_Officials o ON o.OfficialID = v.Official_Link
                LEFT JOIN Ref_Suppliers s ON s.Supplier_ID = v.Supplier_Link
                WHERE v.Voucher_ID IN @VoucherIds;";
            var vouchers = (await conn.QueryAsync<PayableVoucher>(
                    voucherSql, new { VoucherIds = input.Voucher_IDs }, transaction))
                .ToDictionary(v => v.Voucher_ID);

            var missing = input.Voucher_IDs.Where(id => !vouchers.ContainsKey(id)).ToList();
            if (missing.Count > 0)
                throw new InvalidOperationException($"Voucher(s) not found: {string.Join(", ", missing)}.");

            foreach (var v in vouchers.Values)
            {
                var balance = v.Amount - v.PaidSoFar;
                if (balance <= 0)
                    throw new InvalidOperationException($"Voucher {v.Voucher_ID} is already paid in full.");
                v.PayNow = input.Pay_Amounts.TryGetValue(v.Voucher_ID, out var entered) && entered is { } e ? e : balance;
                if (v.PayNow <= 0)
                    throw new InvalidOperationException($"Enter an amount greater than zero to pay on voucher {v.Voucher_ID}.");
                if (v.PayNow > balance)
                    throw new InvalidOperationException(
                        $"Voucher {v.Voucher_ID} only has Sh. {balance:N2} left to pay" +
                        (v.PaidSoFar > 0 ? $" (Sh. {v.PaidSoFar:N2} of Sh. {v.Amount:N2} already paid)" : "") +
                        $", so Sh. {v.PayNow:N2} can't be paid on it.");
            }

            var paymentDate = await ValidatePaymentDateAsync(conn, transaction, input.Payment_Date, input.Withdrawal_Link);

            const string insertPayment = @"
                INSERT INTO Payments (Payment_Date, Payment_Mode, [Description], Amount_Paid, Reference_No, Bank_Account)
                OUTPUT INSERTED.Payment_ID
                VALUES (@PaymentDate, @Payment_Mode, @Description, @Amount, @Reference_No, @Bank_Account);";

            // trg_PaymentAllocations_RequireApproval and trg_PaymentStatusUpdate
            // fire per insert - an unapproved voucher, or an amount exceeding
            // what a voucher is actually worth, rolls back the whole batch.
            const string insertAllocation = @"
                INSERT INTO PaymentAllocations (Payment_ID, Voucher_ID, Allocated_Amount)
                VALUES (@PaymentId, @VoucherId, @Amount);";

            // Optional: if these payments were funded from cash drawn via a
            // specific withdrawal, link each one to it for its own amount.
            const string insertWithdrawalLink = @"
                INSERT INTO WithdrawalPayments (Withdrawal_ID, Payment_ID, Allocated_Amount)
                VALUES (@WithdrawalId, @PaymentId, @Amount);";

            var recorded = new List<RecordedPayment>();

            // Grouped in the order the vouchers were ticked, so the
            // payments come out in a predictable order.
            foreach (var payeeGroup in input.Voucher_IDs.Distinct().Select(id => vouchers[id]).GroupBy(v => v.PayeeKey))
            {
                var lines = payeeGroup.ToList();
                var amount = lines.Sum(v => v.PayNow);

                var paymentId = await conn.ExecuteScalarAsync<string>(insertPayment, new
                {
                    PaymentDate = paymentDate,
                    input.Payment_Mode,
                    input.Description,
                    Amount = amount,
                    input.Reference_No,
                    input.Bank_Account
                }, transaction);

                foreach (var v in lines)
                    await conn.ExecuteAsync(insertAllocation,
                        new { PaymentId = paymentId, VoucherId = v.Voucher_ID, Amount = v.PayNow }, transaction);

                if (!string.IsNullOrWhiteSpace(input.Withdrawal_Link))
                    await conn.ExecuteAsync(insertWithdrawalLink,
                        new { WithdrawalId = input.Withdrawal_Link, PaymentId = paymentId, Amount = amount },
                        transaction);

                recorded.Add(new RecordedPayment
                {
                    Payment_ID = paymentId!,
                    PayeeDisplay = lines[0].PayeeDisplay ?? string.Empty,
                    Amount = amount,
                    Voucher_IDs = lines.Select(v => v.Voucher_ID).ToList()
                });
            }

            transaction.Commit();
            return recorded;
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
        //
        // The label also carries the payment date, the payee and the
        // budget line(s) it covers, so several payments of the same amount
        // and mode can be told apart by when they were made, to whom, and
        // what they were for. A payment spanning several payees shows how
        // many rather than listing them all; one payment can also settle
        // vouchers under more than one budget line, so each distinct
        // budget name is listed.
        const string sql = @"
            SELECT
                p.Payment_ID AS Id,
                p.Payment_ID + ' - ' + FORMAT(p.Payment_Date,'dd MMM yyyy') + ' - ' + FORMAT(p.Amount_Paid,'N2') + ' - ' + p.Payment_Mode
                    + ISNULL(' - ' + CASE
                        WHEN COUNT(DISTINCT COALESCE(o.FullName, s.Business_Name, v.Manual_Payee_Name)) = 1
                            THEN MIN(COALESCE(o.FullName, s.Business_Name, v.Manual_Payee_Name))
                        WHEN COUNT(DISTINCT COALESCE(o.FullName, s.Business_Name, v.Manual_Payee_Name)) > 1
                            THEN CAST(COUNT(DISTINCT COALESCE(o.FullName, s.Business_Name, v.Manual_Payee_Name)) AS VARCHAR(10)) + ' payees'
                        ELSE NULL END, '')
                    + ISNULL(' - ' + (
                        SELECT STRING_AGG(b.Category_Name, ', ')
                        FROM (
                            SELECT DISTINCT LTRIM(RTRIM(bc.Category_Name)) AS Category_Name
                            FROM PaymentAllocations al2
                            INNER JOIN Vouchers v2 ON v2.Voucher_ID = al2.Voucher_ID
                            INNER JOIN Ref_BudgetCodes bc ON bc.Budget_ID = v2.Budget_Link
                            WHERE al2.Payment_ID = p.Payment_ID
                        ) b
                    ), '') AS Label,
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

    // Every payment still waiting on an acknowledgement
    // (PaymentsAwaitingAcknowledgement), with its payee and the earliest
    // withdrawal that funded it - the page groups by that withdrawal,
    // oldest first.
    public async Task<IEnumerable<AcknowledgementBacklogRow>> GetAcknowledgementBacklogAsync()
    {
        using var conn = _db.CreateConnection();

        const string sql = @"
            SELECT a.Payment_ID, a.Payment_Date, a.Payment_Mode, a.Amount_Paid, p.Reference_No,
                   CASE WHEN payee.Names = 1 THEN payee.Name END AS PayeeName,
                   payee.Vouchers AS VoucherCount,
                   wd.Withdrawal_ID, wd.Withdrawal_Date
            FROM PaymentsAwaitingAcknowledgement a
            INNER JOIN Payments p ON p.Payment_ID = a.Payment_ID
            OUTER APPLY (
                SELECT COUNT(DISTINCT COALESCE(o.FullName, s.Business_Name, v.Manual_Payee_Name)) AS Names,
                       MIN(COALESCE(o.FullName, s.Business_Name, v.Manual_Payee_Name)) AS Name,
                       COUNT(*) AS Vouchers
                FROM PaymentAllocations al
                INNER JOIN Vouchers v ON v.Voucher_ID = al.Voucher_ID
                LEFT JOIN Ref_Officials o ON o.OfficialID = v.Official_Link
                LEFT JOIN Ref_Suppliers s ON s.Supplier_ID = v.Supplier_Link
                WHERE al.Payment_ID = a.Payment_ID
            ) payee
            OUTER APPLY (
                SELECT TOP 1 w.Withdrawal_ID, w.Withdrawal_Date
                FROM WithdrawalPayments wp
                INNER JOIN BankWithdrawals w ON w.Withdrawal_ID = wp.Withdrawal_ID
                WHERE wp.Payment_ID = a.Payment_ID
                ORDER BY w.Withdrawal_Date, w.Withdrawal_ID
            ) wd
            ORDER BY CASE WHEN wd.Withdrawal_Date IS NULL THEN 1 ELSE 0 END, wd.Withdrawal_Date, wd.Withdrawal_ID,
                     PayeeName, a.Payment_Date, a.Payment_ID;";

        return await conn.QueryAsync<AcknowledgementBacklogRow>(sql);
    }

    private static readonly HashSet<string> AcknowledgementMethods = new()
    {
        "Signed Voucher", "WhatsApp Confirmation", "M-Pesa Confirmation", "SMS Confirmation",
        "Email Confirmation", "Verbal - Witnessed", "Other"
    };

    // Logs several acknowledgements at once, all or nothing. Each line is
    // checked first - who acknowledged, a known method, a date that is
    // neither in the future nor before the payment - and the whole batch
    // is refused with every problem listed, so nothing half-saves. A
    // payment acknowledged meanwhile by someone else fails on the
    // primary key and rolls the batch back too.
    public async Task<int> RecordAcknowledgementsAsync(IReadOnlyList<AcknowledgementLineInput> lines)
    {
        using var conn = (SqlConnection)_db.CreateConnection();
        conn.Open();

        var paymentDates = (await conn.QueryAsync<(string Id, DateTime Date)>(
                "SELECT Payment_ID, Payment_Date FROM Payments WHERE Payment_ID IN @Ids;",
                new { Ids = lines.Select(l => l.Payment_ID).ToList() }))
            .ToDictionary(p => p.Id, p => p.Date);

        var problems = new List<string>();
        foreach (var l in lines)
        {
            if (!paymentDates.TryGetValue(l.Payment_ID, out var paid))
                problems.Add($"{l.Payment_ID}: payment not found");
            else if (l.Acknowledgement_Date is { } d && (d.Date > DateTime.Today || d.Date < paid.Date))
                problems.Add($"{l.Payment_ID}: date must be between {paid:d MMM yyyy} (paid) and today");
            if (string.IsNullOrWhiteSpace(l.Acknowledged_By))
                problems.Add($"{l.Payment_ID}: enter who acknowledged it");
            if (l.Method is null || !AcknowledgementMethods.Contains(l.Method))
                problems.Add($"{l.Payment_ID}: choose a method");
        }
        if (problems.Count > 0)
            throw new InvalidOperationException("Nothing was logged. " + string.Join("; ", problems) + ".");

        using var transaction = conn.BeginTransaction();
        const string sql = @"
            INSERT INTO Payment_Acknowledgements (Payment_ID, Acknowledged_By, Acknowledgement_Date, Method, Notes)
            VALUES (@Payment_ID, @By, @AckDate, @Method, @Notes);";
        try
        {
            foreach (var l in lines)
                await conn.ExecuteAsync(sql, new
                {
                    l.Payment_ID,
                    By = l.Acknowledged_By!.Trim(),
                    AckDate = (l.Acknowledgement_Date ?? DateTime.Today).Date,
                    l.Method,
                    Notes = string.IsNullOrWhiteSpace(l.Notes) ? null : l.Notes.Trim()
                }, transaction);
            transaction.Commit();
            return lines.Count;
        }
        catch (SqlException ex) when (ex.Number is 2627 or 2601)
        {
            transaction.Rollback();
            throw new InvalidOperationException("Nothing was logged: one of the ticked payments was acknowledged in the meantime. Reload the page and try again.");
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
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
