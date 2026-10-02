using System.Globalization;
using Dapper;
using Microsoft.Data.SqlClient;
using UASU_VoucherApprovals.Data;
using UASU_VoucherApprovals.Models;
using UASU_VoucherApprovals.Utilities;

namespace UASU_VoucherApprovals.Services;

// Cash floats (SQL/032): cash handed to one official for a purpose, then
// accounted for with receipts. Receipts reach the books only through the
// normal voucher pipeline - retired onto Expense vouchers paid to the
// custodian, approved by Chairman + Chapter Secretary, then settled as a
// Cash payment linked to the float's withdrawal - so the cash book,
// budget performance and certification pick them up with no special case.
public interface ICashFloatService
{
    Task<IEnumerable<CashFloatRow>> GetFloatsAsync();
    Task<CashFloatRow?> GetFloatAsync(string floatId);
    Task<IEnumerable<CashFloatRow>> GetPositionsAsOfAsync(DateTime asOf);

    Task<IEnumerable<SimpleOption>> GetCustodianOptionsAsync();
    Task<IEnumerable<SimpleOption>> GetExpenseBudgetCodesAsync();
    Task<IEnumerable<SimpleOption>> GetWithdrawalOptionsAsync();

    Task<string> IssueAsync(CashFloatInputModel input, string issuedByOfficialId);

    Task<IEnumerable<CashFloatReceiptRow>> GetReceiptsAsync(string floatId);
    Task AddReceiptAsync(CashFloatReceiptInputModel input, string recordedByOfficialId);
    Task DeleteReceiptAsync(string floatId, int receiptLineId);
    Task<CashFloatAttachment?> GetReceiptAttachmentAsync(int receiptLineId);

    Task<IReadOnlyList<string>> RetireAsync(string floatId, IReadOnlyCollection<int> receiptLineIds);
    Task<IEnumerable<CashFloatVoucherRow>> GetRetirementVouchersAsync(string floatId);
    Task<IReadOnlyList<string>> SettleAsync(string floatId, IReadOnlyCollection<string> voucherIds);

    Task<IEnumerable<CashFloatReturnRow>> GetReturnsAsync(string floatId);
    Task RecordReturnAsync(CashFloatReturnInputModel input, string recordedByOfficialId);

    Task CloseAsync(string floatId);
}

public class CashFloatService : ICashFloatService
{
    private const long MaxAttachmentBytes = 5 * 1024 * 1024;

    // Vouchers.Description is NVARCHAR(1000) (SQL/029).
    private const int MaxVoucherDescription = 1000;

    private readonly IDbConnectionFactory _db;
    private readonly IVoucherService _voucherService;

    public CashFloatService(IDbConnectionFactory db, IVoucherService voucherService)
    {
        _db = db;
        _voucherService = voucherService;
    }

    // ------------------------------------------------------------------
    // Floats
    // ------------------------------------------------------------------

    // Every figure comes from fn_CashFloatPosition, the same definition
    // the database triggers and Monthly Certification use.
    private const string PositionSql = @"
        SELECT p.Float_ID, p.Float_Type, p.Purpose, p.Custodian_ID, o.FullName AS CustodianName,
               p.Withdrawal_ID, p.Issue_Date, p.Closed_Date, f.Notes,
               p.Issued, p.Spent, p.Settled, p.AwaitingVoucher, p.Returned,
               p.CashWithCustodian, p.BookOutstanding
        FROM fn_CashFloatPosition(@AsOf) p
        INNER JOIN CashFloats f ON f.Float_ID = p.Float_ID
        INNER JOIN Ref_Officials o ON o.OfficialID = p.Custodian_ID";

    // fn_CashFloatPosition's "everything so far" date.
    private static readonly DateTime AllTime = new(9999, 12, 31);

    public async Task<IEnumerable<CashFloatRow>> GetFloatsAsync()
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<CashFloatRow>(
            PositionSql + " ORDER BY CASE WHEN p.Closed_Date IS NULL THEN 0 ELSE 1 END, p.Issue_Date DESC, p.Float_ID DESC;",
            new { AsOf = AllTime });
    }

    public async Task<CashFloatRow?> GetFloatAsync(string floatId)
    {
        using var conn = _db.CreateConnection();
        return await conn.QuerySingleOrDefaultAsync<CashFloatRow>(
            PositionSql + " WHERE p.Float_ID = @FloatId;", new { AsOf = AllTime, FloatId = floatId });
    }

    // Floats as they stood at a date - for Monthly Certification. A float
    // closed after @AsOf still counts as open then.
    public async Task<IEnumerable<CashFloatRow>> GetPositionsAsOfAsync(DateTime asOf)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<CashFloatRow>(
            PositionSql + " WHERE p.Closed_Date IS NULL OR p.Closed_Date > @AsOf ORDER BY p.Issue_Date, p.Float_ID;",
            new { AsOf = asOf.Date });
    }

    // Cash is only ever handed to a sitting official.
    public async Task<IEnumerable<SimpleOption>> GetCustodianOptionsAsync()
    {
        using var conn = _db.CreateConnection();
        const string sql = @"
            SELECT OfficialID AS Id, FullName + ' (' + Role + ')' AS Label
            FROM Ref_Officials
            WHERE IsCurrent = 1
            ORDER BY FullName;";
        return await conn.QueryAsync<SimpleOption>(sql);
    }

    public async Task<IEnumerable<SimpleOption>> GetExpenseBudgetCodesAsync()
    {
        using var conn = _db.CreateConnection();
        const string sql = @"
            SELECT Budget_ID AS Id, Category_Name AS Label
            FROM Ref_BudgetCodes
            WHERE Category_Type = 'Expense'
            ORDER BY Category_Name;";
        return await conn.QueryAsync<SimpleOption>(sql);
    }

    // Withdrawals with cash still free: not yet tied to payments or charges
    // (BankReconciliation) and not already out in another float.
    private const string WithdrawalFreeSql = @"
        SELECT br.Withdrawal_ID, br.Withdrawal_Date,
               br.UnallocatedBalance - ISNULL(fl.InFloats, 0) AS Free
        FROM BankReconciliation br
        LEFT JOIN (
            SELECT Withdrawal_ID, SUM(BookOutstanding) AS InFloats
            FROM fn_CashFloatPosition('9999-12-31')
            WHERE Withdrawal_ID IS NOT NULL
            GROUP BY Withdrawal_ID
        ) fl ON fl.Withdrawal_ID = br.Withdrawal_ID";

    public async Task<IEnumerable<SimpleOption>> GetWithdrawalOptionsAsync()
    {
        using var conn = _db.CreateConnection();
        var rows = await conn.QueryAsync<WithdrawalFree>(
            WithdrawalFreeSql + " WHERE br.UnallocatedBalance - ISNULL(fl.InFloats, 0) > 0 ORDER BY br.Withdrawal_Date DESC;");
        return rows.Select(r => new SimpleOption
        {
            Id = r.Withdrawal_ID,
            Label = $"{r.Withdrawal_ID} - {r.Withdrawal_Date:yyyy-MM-dd} - Sh.{r.Free:N2} free"
        });
    }

    private class WithdrawalFree
    {
        public string Withdrawal_ID { get; set; } = string.Empty;
        public DateTime Withdrawal_Date { get; set; }
        public decimal Free { get; set; }
    }

    public async Task<string> IssueAsync(CashFloatInputModel input, string issuedByOfficialId)
    {
        using var conn = _db.CreateConnection();

        var withdrawalId = string.IsNullOrWhiteSpace(input.Withdrawal_ID) ? null : input.Withdrawal_ID;
        if (withdrawalId is not null)
        {
            var free = await conn.QuerySingleOrDefaultAsync<WithdrawalFree>(
                WithdrawalFreeSql + " WHERE br.Withdrawal_ID = @WithdrawalId;", new { WithdrawalId = withdrawalId });
            if (free is null)
                throw new InvalidOperationException($"Withdrawal {withdrawalId} was not found.");
            if (input.Amount > free.Free)
                throw new InvalidOperationException(
                    $"Withdrawal {withdrawalId} only has Sh.{free.Free:N2} not already paid out or held in another float.");
            if (input.Issue_Date.Date < free.Withdrawal_Date.Date)
                throw new InvalidOperationException("A float can't be issued before the withdrawal that funds it.");
        }

        const string sql = @"
            INSERT INTO CashFloats (Float_Type, Purpose, Custodian_ID, Withdrawal_ID, Issue_Date, Amount, Issued_By, Notes)
            OUTPUT INSERTED.Float_ID
            VALUES (@Float_Type, @Purpose, @Custodian_ID, @WithdrawalId, @Issue_Date, @Amount, @IssuedBy, @Notes);";

        return (await conn.ExecuteScalarAsync<string>(sql, new
        {
            input.Float_Type,
            Purpose = input.Purpose.Trim(),
            input.Custodian_ID,
            WithdrawalId = withdrawalId,
            input.Issue_Date,
            input.Amount,
            IssuedBy = issuedByOfficialId,
            input.Notes
        }))!;
    }

    // ------------------------------------------------------------------
    // Receipts
    // ------------------------------------------------------------------

    private const string RetirementStateSql = @"
        CASE
            WHEN {0}.Voucher_ID IS NULL THEN 'Not retired'
            WHEN EXISTS (SELECT 1 FROM Voucher_Approvals va
                         WHERE va.Voucher_ID = {0}.Voucher_ID AND va.Approval_Status = 'Rejected') THEN 'Rejected'
            WHEN EXISTS (SELECT 1 FROM PaymentAllocations pa WHERE pa.Voucher_ID = {0}.Voucher_ID) THEN 'Settled'
            WHEN EXISTS (SELECT 1 FROM VoucherApprovalStatus vas
                         WHERE vas.Voucher_ID = {0}.Voucher_ID AND vas.FullyApproved = 1) THEN 'Approved - not settled'
            ELSE 'Awaiting approval'
        END";

    public async Task<IEnumerable<CashFloatReceiptRow>> GetReceiptsAsync(string floatId)
    {
        using var conn = _db.CreateConnection();
        var sql = $@"
            SELECT cr.Receipt_Line_ID, cr.Float_ID, cr.Receipt_Date, cr.Amount, cr.Vendor, cr.Receipt_No,
                   cr.[Description], cr.Budget_Link, bc.Category_Name, cr.Voucher_ID,
                   CASE WHEN cr.Attachment_Data IS NOT NULL THEN 1 ELSE 0 END AS HasAttachment,
                   {string.Format(RetirementStateSql, "cr")} AS RetirementState
            FROM CashFloatReceipts cr
            LEFT JOIN Ref_BudgetCodes bc ON bc.Budget_ID = cr.Budget_Link
            WHERE cr.Float_ID = @FloatId
            ORDER BY cr.Receipt_Date, cr.Receipt_Line_ID;";
        return await conn.QueryAsync<CashFloatReceiptRow>(sql, new { FloatId = floatId });
    }

    // trg_CashFloatReceipts_Validate rejects a receipt that would take the
    // float past what was issued, or lands on a closed float.
    public async Task AddReceiptAsync(CashFloatReceiptInputModel input, string recordedByOfficialId)
    {
        byte[]? attachmentData = null;
        string? attachmentFileName = null;
        string? attachmentContentType = null;

        if (input.Attachment is { Length: > 0 } file)
        {
            if (file.Length > MaxAttachmentBytes)
                throw new InvalidOperationException("Receipt scan must be 5MB or smaller.");

            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            attachmentData = ms.ToArray();

            if (!FileSignature.TryDetectImageOrPdf(attachmentData, out attachmentContentType))
                throw new InvalidOperationException("Receipt scan must be a JPEG, PNG or PDF.");

            attachmentFileName = Path.GetFileName(file.FileName);
        }

        using var conn = _db.CreateConnection();

        var issueDate = await conn.ExecuteScalarAsync<DateTime?>(
            "SELECT Issue_Date FROM CashFloats WHERE Float_ID = @Float_ID;", new { input.Float_ID });
        if (issueDate is null)
            throw new InvalidOperationException($"Float {input.Float_ID} was not found.");
        if (input.Receipt_Date.Date < issueDate.Value.Date)
            throw new InvalidOperationException("A receipt can't be dated before its float was issued.");

        const string sql = @"
            INSERT INTO CashFloatReceipts
                (Float_ID, Receipt_Date, Amount, Vendor, Receipt_No, [Description], Budget_Link, Recorded_By,
                 Attachment_Data, Attachment_FileName, Attachment_ContentType)
            VALUES
                (@Float_ID, @Receipt_Date, @Amount, @Vendor, @Receipt_No, @Description, @Budget_Link, @RecordedBy,
                 @AttachmentData, @AttachmentFileName, @AttachmentContentType);";

        await conn.ExecuteAsync(sql, new
        {
            input.Float_ID,
            input.Receipt_Date,
            input.Amount,
            Vendor = input.Vendor.Trim(),
            Receipt_No = string.IsNullOrWhiteSpace(input.Receipt_No) ? null : input.Receipt_No.Trim(),
            Description = input.Description.Trim(),
            input.Budget_Link,
            RecordedBy = recordedByOfficialId,
            AttachmentData = attachmentData,
            AttachmentFileName = attachmentFileName,
            AttachmentContentType = attachmentContentType
        });
    }

    // Only for a receipt entered by mistake. One on a live retirement
    // voucher is refused by trg_CashFloatReceipts_Validate.
    public async Task DeleteReceiptAsync(string floatId, int receiptLineId)
    {
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync(
            "DELETE FROM CashFloatReceipts WHERE Receipt_Line_ID = @Id AND Float_ID = @FloatId;",
            new { Id = receiptLineId, FloatId = floatId });
    }

    public async Task<CashFloatAttachment?> GetReceiptAttachmentAsync(int receiptLineId)
    {
        using var conn = _db.CreateConnection();
        const string sql = @"
            SELECT Attachment_Data, Attachment_FileName, Attachment_ContentType
            FROM CashFloatReceipts
            WHERE Receipt_Line_ID = @Id AND Attachment_Data IS NOT NULL;";
        return await conn.QuerySingleOrDefaultAsync<CashFloatAttachment>(sql, new { Id = receiptLineId });
    }

    // ------------------------------------------------------------------
    // Retire: receipts -> one Expense voucher per budget line, payee = the
    // custodian. The vouchers then wait for normal approval.
    // ------------------------------------------------------------------

    public async Task<IReadOnlyList<string>> RetireAsync(string floatId, IReadOnlyCollection<int> receiptLineIds)
    {
        if (receiptLineIds.Count == 0)
            throw new InvalidOperationException("Tick at least one receipt to retire.");

        using var conn = (SqlConnection)_db.CreateConnection();
        conn.Open();
        using var transaction = conn.BeginTransaction();

        try
        {
            var flt = await conn.QuerySingleOrDefaultAsync<FloatHeader>(
                          "SELECT Purpose, Custodian_ID FROM CashFloats WHERE Float_ID = @FloatId;",
                          new { FloatId = floatId }, transaction)
                      ?? throw new InvalidOperationException($"Float {floatId} was not found.");

            // Re-read the receipts rather than trusting the form: each must
            // belong to this float and still be free to retire.
            var sql = $@"
                SELECT cr.Receipt_Line_ID, cr.Float_ID, cr.Receipt_Date, cr.Amount, cr.Vendor, cr.Receipt_No,
                       cr.[Description], cr.Budget_Link, bc.Category_Name, cr.Voucher_ID,
                       {string.Format(RetirementStateSql, "cr")} AS RetirementState
                FROM CashFloatReceipts cr
                LEFT JOIN Ref_BudgetCodes bc ON bc.Budget_ID = cr.Budget_Link
                WHERE cr.Float_ID = @FloatId AND cr.Receipt_Line_ID IN @Ids
                ORDER BY cr.Receipt_Date, cr.Receipt_Line_ID;";
            var receipts = (await conn.QueryAsync<CashFloatReceiptRow>(
                sql, new { FloatId = floatId, Ids = receiptLineIds }, transaction)).ToList();

            if (receipts.Count != receiptLineIds.Count)
                throw new InvalidOperationException("Nothing was retired: some of the ticked receipts aren't on this float any more.");
            var taken = receipts.Where(r => !r.CanRetire).ToList();
            if (taken.Count > 0)
                throw new InvalidOperationException("Nothing was retired: " + string.Join(", ",
                    taken.Select(r => $"receipt {r.Receipt_No ?? r.Receipt_Line_ID.ToString()} is already on voucher {r.Voucher_ID}")) + ".");

            const string insertVoucher = @"
                INSERT INTO Vouchers
                    (VoucherDate, Transaction_Type, Budget_Link, Official_Link, Amount,
                     [Description], Payee_Category, Is_Travel_Expense)
                OUTPUT INSERTED.Voucher_ID
                VALUES
                    (@VoucherDate, 'Expense', @BudgetLink, @OfficialId, @Amount,
                     @Description, 'Official', 0);";

            // One UPDATE per voucher, so trg_CashFloatReceipts_Validate sees
            // the voucher's full set of receipts when it checks the total.
            const string linkReceipts = @"
                UPDATE CashFloatReceipts SET Voucher_ID = @VoucherId
                WHERE Float_ID = @FloatId AND Receipt_Line_ID IN @Ids;";

            var voucherIds = new List<string>();
            foreach (var group in receipts.GroupBy(r => r.Budget_Link))
            {
                var lines = group.ToList();
                var voucherId = await conn.ExecuteScalarAsync<string>(insertVoucher, new
                {
                    VoucherDate = DateTime.Today,
                    BudgetLink = group.Key,
                    OfficialId = flt.Custodian_ID,
                    Amount = lines.Sum(r => r.Amount),
                    Description = BuildRetirementDescription(floatId, flt.Purpose, lines)
                }, transaction);

                await conn.ExecuteAsync(linkReceipts, new
                {
                    VoucherId = voucherId,
                    FloatId = floatId,
                    Ids = lines.Select(r => r.Receipt_Line_ID)
                }, transaction);

                voucherIds.Add(voucherId!);
            }

            transaction.Commit();
            return voucherIds;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    private class FloatHeader
    {
        public string Purpose { get; set; } = string.Empty;
        public string Custodian_ID { get; set; } = string.Empty;
    }

    // What the approvers read on the Approvals page, so it names the float
    // and every receipt behind the amount. Falls back to a count when the
    // itemised list won't fit in the voucher's description.
    private static string BuildRetirementDescription(string floatId, string purpose, IReadOnlyList<CashFloatReceiptRow> lines)
    {
        var head = $"Cash float {floatId} ({purpose}) - retirement of {lines.Count} receipt{(lines.Count == 1 ? "" : "s")}: ";
        var items = string.Join("; ", lines.Select(r =>
            $"{r.Receipt_Date.ToString("d MMM", CultureInfo.InvariantCulture)} {r.Vendor}" +
            (string.IsNullOrWhiteSpace(r.Receipt_No) ? "" : $" #{r.Receipt_No}") +
            $" - {r.Description} Sh.{r.Amount:N2}"));

        var full = head + items;
        if (full.Length <= MaxVoucherDescription)
            return full;

        var shortForm = $"Cash float {floatId} ({purpose}) - retirement of {lines.Count} receipts dated "
                        + $"{lines.Min(r => r.Receipt_Date):d MMM} to {lines.Max(r => r.Receipt_Date):d MMM yyyy}; itemised on the float statement.";
        return shortForm.Length <= MaxVoucherDescription ? shortForm : shortForm[..MaxVoucherDescription];
    }

    public async Task<IEnumerable<CashFloatVoucherRow>> GetRetirementVouchersAsync(string floatId)
    {
        using var conn = _db.CreateConnection();
        var sql = $@"
            SELECT v.Voucher_ID, v.VoucherDate, v.Amount, bc.Category_Name,
                   COUNT(cr.Receipt_Line_ID) AS ReceiptCount,
                   {string.Format(RetirementStateSql, "v")} AS RetirementState,
                   (SELECT TOP 1 pa.Payment_ID FROM PaymentAllocations pa WHERE pa.Voucher_ID = v.Voucher_ID) AS Payment_ID
            FROM Vouchers v
            INNER JOIN CashFloatReceipts cr ON cr.Voucher_ID = v.Voucher_ID
            LEFT JOIN Ref_BudgetCodes bc ON bc.Budget_ID = v.Budget_Link
            WHERE cr.Float_ID = @FloatId
            GROUP BY v.Voucher_ID, v.VoucherDate, v.Amount, bc.Category_Name
            ORDER BY v.VoucherDate, v.Voucher_ID;";
        return await conn.QueryAsync<CashFloatVoucherRow>(sql, new { FloatId = floatId });
    }

    // ------------------------------------------------------------------
    // Settle: approved retirement vouchers -> ordinary Cash payments, one
    // per voucher, linked to the float's withdrawal. RecordPaymentAsync's
    // own triggers (trg_PaymentAllocations_RequireApproval etc.) still apply.
    // ------------------------------------------------------------------

    public async Task<IReadOnlyList<string>> SettleAsync(string floatId, IReadOnlyCollection<string> voucherIds)
    {
        if (voucherIds.Count == 0)
            throw new InvalidOperationException("Tick at least one approved voucher to settle.");

        var flt = await GetFloatAsync(floatId)
                  ?? throw new InvalidOperationException($"Float {floatId} was not found.");

        var vouchers = (await GetRetirementVouchersAsync(floatId))
            .Where(v => voucherIds.Contains(v.Voucher_ID))
            .ToList();

        if (vouchers.Count != voucherIds.Count)
            throw new InvalidOperationException("Nothing was settled: some of the ticked vouchers don't retire this float's receipts.");
        var notReady = vouchers.Where(v => v.RetirementState != "Approved - not settled").ToList();
        if (notReady.Count > 0)
            throw new InvalidOperationException("Nothing was settled: " + string.Join(", ",
                notReady.Select(v => $"{v.Voucher_ID} is {v.RetirementState.ToLowerInvariant()}")) + ".");

        return await _voucherService.RecordPaymentAsync(new PaymentInputModel
        {
            Voucher_IDs = vouchers.Select(v => v.Voucher_ID).ToList(),
            Amount_Paid = vouchers.Sum(v => v.Amount),
            Payment_Mode = "Cash",
            Description = $"Settled from cash float {floatId} ({flt.Purpose}) held by {flt.CustodianName}",
            Withdrawal_Link = flt.Withdrawal_ID
        });
    }

    // ------------------------------------------------------------------
    // Returns and closing
    // ------------------------------------------------------------------

    public async Task<IEnumerable<CashFloatReturnRow>> GetReturnsAsync(string floatId)
    {
        using var conn = _db.CreateConnection();
        const string sql = @"
            SELECT r.Return_ID, r.Return_Date, r.Amount, r.Notes, o.FullName AS RecordedByName
            FROM CashFloatReturns r
            INNER JOIN Ref_Officials o ON o.OfficialID = r.Recorded_By
            WHERE r.Float_ID = @FloatId
            ORDER BY r.Return_Date, r.Return_ID;";
        return await conn.QueryAsync<CashFloatReturnRow>(sql, new { FloatId = floatId });
    }

    // trg_CashFloatReturns_Validate rejects a return larger than what the
    // custodian should still be holding.
    public async Task RecordReturnAsync(CashFloatReturnInputModel input, string recordedByOfficialId)
    {
        using var conn = _db.CreateConnection();
        const string sql = @"
            INSERT INTO CashFloatReturns (Float_ID, Return_Date, Amount, Notes, Recorded_By)
            VALUES (@Float_ID, @Return_Date, @Amount, @Notes, @RecordedBy);";
        await conn.ExecuteAsync(sql, new
        {
            input.Float_ID,
            input.Return_Date,
            input.Amount,
            input.Notes,
            RecordedBy = recordedByOfficialId
        });
    }

    // trg_CashFloats_Validate refuses unless every shilling is spent or
    // returned and every receipt has been settled.
    public async Task CloseAsync(string floatId)
    {
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync(
            "UPDATE CashFloats SET Closed_Date = CAST(GETDATE() AS DATE) WHERE Float_ID = @FloatId AND Closed_Date IS NULL;",
            new { FloatId = floatId });
    }
}
