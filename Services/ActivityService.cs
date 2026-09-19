using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using UASU_VoucherApprovals.Data;
using UASU_VoucherApprovals.Models;
using UASU_VoucherApprovals.Utilities;

namespace UASU_VoucherApprovals.Services;

public interface IActivityService
{
    Task<IReadOnlyList<ActivityOfficialOption>> GetOfficialsAsync();
    Task<string> CreateActivityAsync(ActivityInputModel input, string loggedBy);
    Task<IReadOnlyList<ActivityView>> GetMyActivitiesAsync(string officialId);
    Task<int> CountRecordedForAsync(string officialId);
    Task<ActivityEvidenceFile?> GetEvidenceAsync(int evidenceId);

    Task<IReadOnlyList<OpenActivityLine>> GetOpenLinesAsync(DateTime? monthStart);
    Task<IReadOnlyList<MonthCategoryCount>> GetMonthCountsAsync(DateTime monthStart);
    Task<IReadOnlyList<string>> GetOpenMonthsAsync();

    // Pure checks/planning, shared by the preview and the real thing so
    // what the treasurer sees is exactly what gets created.
    (List<(OpenActivityLine Line, LineDecisionInput Decision)> Pay, List<(OpenActivityLine Line, LineDecisionInput Decision)> NotPayable, List<string> Errors)
        ValidateDecisions(IEnumerable<LineDecisionInput> decisions, IReadOnlyList<OpenActivityLine> openLines, IReadOnlyDictionary<string, string> budgetLabels, string decidedBy);
    List<VoucherPlan> PlanVouchers(IEnumerable<(OpenActivityLine Line, LineDecisionInput Decision)> pay, IReadOnlyDictionary<string, string> budgetLabels);

    Task<DecisionResult> DecideAsync(IReadOnlyList<LineDecisionInput> decisions, string decidedBy, DateTime voucherDate, IReadOnlyDictionary<string, string> budgetLabels);
}

public class ActivityService : IActivityService
{
    private const long MaxEvidenceBytes = 5 * 1024 * 1024;
    private const int MaxEvidenceFiles = 5;

    private readonly IDbConnectionFactory _db;

    public ActivityService(IDbConnectionFactory db)
    {
        _db = db;
    }

    // ------------------------------------------------------------------
    // Logging
    // ------------------------------------------------------------------

    public async Task<IReadOnlyList<ActivityOfficialOption>> GetOfficialsAsync()
    {
        using var conn = _db.CreateConnection();
        const string sql = @"
            SELECT o.OfficialID, o.FullName, o.Role,
                   CASE WHEN u.LastLogin IS NULL THEN 1 ELSE 0 END AS NotSignedIn
            FROM Ref_Officials o
            LEFT JOIN Users u ON u.OfficialID = o.OfficialID
            WHERE o.IsCurrent = 1
            ORDER BY o.FullName;";
        return (await conn.QueryAsync<ActivityOfficialOption>(sql)).ToList();
    }

    public async Task<string> CreateActivityAsync(ActivityInputModel input, string loggedBy)
    {
        var participantIds = input.ParticipantIds.Where(i => !string.IsNullOrWhiteSpace(i)).Distinct().ToList();
        if (participantIds.Count == 0)
            throw new InvalidOperationException("Tick at least one official who took part.");
        if (input.Activity_Date.Date > DateTime.Today)
            throw new InvalidOperationException("The activity date cannot be in the future.");
        if (!ActivityLists.Categories.Contains(input.Category))
            throw new InvalidOperationException("Choose a type of activity from the list.");
        if (!ActivityLists.AuthorisedBy.Contains(input.Authorised_By))
            throw new InvalidOperationException("Choose who authorised it from the list.");

        var evidence = new List<(string Name, string ContentType, byte[] Data)>();
        var files = input.Evidence.Where(f => f is { Length: > 0 }).ToList();
        if (files.Count > MaxEvidenceFiles)
            throw new InvalidOperationException($"Attach at most {MaxEvidenceFiles} evidence files.");
        foreach (var file in files)
        {
            if (file.Length > MaxEvidenceBytes)
                throw new InvalidOperationException($"\"{Path.GetFileName(file.FileName)}\" is larger than 5MB.");

            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            var data = ms.ToArray();

            // Sniff the real bytes, not the browser-supplied type or the
            // extension - both are fully controlled by the uploader.
            if (!TryDetectContentType(data, out var contentType))
                throw new InvalidOperationException($"\"{Path.GetFileName(file.FileName)}\" must be a JPEG, PNG or PDF file.");

            evidence.Add((Path.GetFileName(file.FileName), contentType!, data));
        }

        using var conn = (SqlConnection)_db.CreateConnection();
        conn.Open();
        using var transaction = conn.BeginTransaction();
        try
        {
            var validCount = await conn.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM Ref_Officials WHERE IsCurrent = 1 AND OfficialID IN @Ids;",
                new { Ids = participantIds }, transaction);
            if (validCount != participantIds.Count)
                throw new InvalidOperationException("One of the officials ticked is not a current official.");

            const string insertActivity = @"
                INSERT INTO Activities
                    (Activity_Date, Category, Title, Details, Place, Days_Spent, Others_Involved,
                     Authorised_By, Expenses_Amount, Expenses_Note, Logged_By)
                OUTPUT INSERTED.Activity_ID
                VALUES
                    (@Activity_Date, @Category, @Title, @Details, @Place, @Days_Spent, @Others_Involved,
                     @Authorised_By, @Expenses_Amount, @Expenses_Note, @LoggedBy);";

            var activityId = await conn.ExecuteScalarAsync<string>(insertActivity, new
            {
                Activity_Date = input.Activity_Date.Date,
                input.Category,
                Title = input.Title.Trim(),
                Details = input.Details.Trim(),
                Place = string.IsNullOrWhiteSpace(input.Place) ? null : input.Place.Trim(),
                input.Days_Spent,
                Others_Involved = string.IsNullOrWhiteSpace(input.Others_Involved) ? null : input.Others_Involved.Trim(),
                input.Authorised_By,
                input.Expenses_Amount,
                Expenses_Note = string.IsNullOrWhiteSpace(input.Expenses_Note) ? null : input.Expenses_Note.Trim(),
                LoggedBy = loggedBy
            }, transaction);

            await conn.ExecuteAsync(
                "INSERT INTO ActivityParticipants (Activity_ID, OfficialID) VALUES (@ActivityId, @OfficialId);",
                participantIds.Select(id => new { ActivityId = activityId, OfficialId = id }), transaction);

            foreach (var e in evidence)
            {
                await conn.ExecuteAsync(
                    "INSERT INTO ActivityEvidence (Activity_ID, File_Name, Content_Type, File_Data) VALUES (@ActivityId, @Name, @ContentType, @Data);",
                    new { ActivityId = activityId, e.Name, e.ContentType, e.Data }, transaction);
            }

            transaction.Commit();
            return activityId!;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public async Task<IReadOnlyList<ActivityView>> GetMyActivitiesAsync(string officialId)
    {
        using var conn = _db.CreateConnection();

        const string activitiesSql = @"
            SELECT a.Activity_ID, a.Activity_Date, a.Category, a.Title, a.Details, a.Place, a.Days_Spent,
                   a.Others_Involved, a.Authorised_By, a.Expenses_Amount, a.Expenses_Note,
                   a.Logged_By, lo.FullName AS LoggedByName
            FROM Activities a
            INNER JOIN Ref_Officials lo ON lo.OfficialID = a.Logged_By
            WHERE a.Logged_By = @OfficialId
               OR EXISTS (SELECT 1 FROM ActivityParticipants p WHERE p.Activity_ID = a.Activity_ID AND p.OfficialID = @OfficialId)
            ORDER BY a.Activity_Date DESC, a.Activity_ID DESC;";

        var activities = (await conn.QueryAsync<ActivityView>(activitiesSql, new { OfficialId = officialId })).ToList();
        if (activities.Count == 0)
            return activities;

        var ids = activities.Select(a => a.Activity_ID).ToList();

        const string linesSql = @"
            SELECT p.Activity_ID, p.OfficialID, o.FullName,
                   CASE WHEN u.LastLogin IS NULL THEN 1 ELSE 0 END AS NotSignedIn,
                   p.Decision, p.Reason, p.Voucher_ID,
                   CASE
                       WHEN p.Voucher_ID IS NULL THEN NULL
                       WHEN EXISTS (SELECT 1 FROM Voucher_Approvals va WHERE va.Voucher_ID = p.Voucher_ID AND va.Approval_Status = 'Rejected') THEN 'Rejected'
                       WHEN EXISTS (SELECT 1 FROM PaymentAllocations pa WHERE pa.Voucher_ID = p.Voucher_ID) THEN 'Paid'
                       WHEN vas.FullyApproved = 1 THEN 'Approved'
                       ELSE 'Awaiting approval'
                   END AS VoucherStatus
            FROM ActivityParticipants p
            INNER JOIN Ref_Officials o ON o.OfficialID = p.OfficialID
            LEFT JOIN Users u ON u.OfficialID = p.OfficialID
            LEFT JOIN VoucherApprovalStatus vas ON vas.Voucher_ID = p.Voucher_ID
            WHERE p.Activity_ID IN @Ids
            ORDER BY o.FullName;";
        var lines = (await conn.QueryAsync<ActivityLineView>(linesSql, new { Ids = ids })).ToLookup(l => l.Activity_ID);

        var evidence = (await conn.QueryAsync<ActivityEvidenceInfo>(
            "SELECT Evidence_ID, Activity_ID, File_Name FROM ActivityEvidence WHERE Activity_ID IN @Ids ORDER BY Evidence_ID;",
            new { Ids = ids })).ToLookup(e => e.Activity_ID);

        foreach (var a in activities)
        {
            a.Lines = lines[a.Activity_ID].ToList();
            a.Evidence = evidence[a.Activity_ID].ToList();
        }

        return activities;
    }

    // Activities someone else logged with this official on them - backs the
    // "colleagues recorded N activities for you" notice on Log Activity.
    public async Task<int> CountRecordedForAsync(string officialId)
    {
        using var conn = _db.CreateConnection();
        return await conn.ExecuteScalarAsync<int>(@"
            SELECT COUNT(*)
            FROM ActivityParticipants p
            INNER JOIN Activities a ON a.Activity_ID = p.Activity_ID
            WHERE p.OfficialID = @OfficialId AND a.Logged_By <> @OfficialId;",
            new { OfficialId = officialId });
    }

    public async Task<ActivityEvidenceFile?> GetEvidenceAsync(int evidenceId)
    {
        using var conn = _db.CreateConnection();
        return await conn.QuerySingleOrDefaultAsync<ActivityEvidenceFile>(
            "SELECT File_Data, File_Name, Content_Type FROM ActivityEvidence WHERE Evidence_ID = @Id;",
            new { Id = evidenceId });
    }

    // ------------------------------------------------------------------
    // Treasury decisions
    // ------------------------------------------------------------------

    public async Task<IReadOnlyList<OpenActivityLine>> GetOpenLinesAsync(DateTime? monthStart)
    {
        using var conn = _db.CreateConnection();
        const string sql = @"
            SELECT p.Activity_ID, p.OfficialID, o.FullName, o.Role,
                   CASE WHEN u.LastLogin IS NULL THEN 1 ELSE 0 END AS NotSignedIn,
                   a.Activity_Date, a.Category, a.Title, a.Days_Spent,
                   (SELECT COUNT(*) FROM ActivityParticipants px WHERE px.Activity_ID = a.Activity_ID) AS ParticipantCount,
                   a.Expenses_Amount, a.Expenses_Note, lo.FullName AS LoggedByName
            FROM ActivityParticipants p
            INNER JOIN Activities a ON a.Activity_ID = p.Activity_ID
            INNER JOIN Ref_Officials o ON o.OfficialID = p.OfficialID
            INNER JOIN Ref_Officials lo ON lo.OfficialID = a.Logged_By
            LEFT JOIN Users u ON u.OfficialID = p.OfficialID
            WHERE p.Decision IS NULL
              AND (@MonthStart IS NULL OR (a.Activity_Date >= @MonthStart AND a.Activity_Date < DATEADD(MONTH, 1, @MonthStart)))
            ORDER BY o.FullName, a.Activity_Date, a.Activity_ID;";
        return (await conn.QueryAsync<OpenActivityLine>(sql, new { MonthStart = monthStart })).ToList();
    }

    public async Task<IReadOnlyList<MonthCategoryCount>> GetMonthCountsAsync(DateTime monthStart)
    {
        using var conn = _db.CreateConnection();
        const string sql = @"
            SELECT p.OfficialID, a.Category, COUNT(*) AS Total,
                   SUM(CASE WHEN p.Decision = 'Pay' THEN 1 ELSE 0 END) AS Paid
            FROM ActivityParticipants p
            INNER JOIN Activities a ON a.Activity_ID = p.Activity_ID
            WHERE a.Activity_Date >= @MonthStart AND a.Activity_Date < DATEADD(MONTH, 1, @MonthStart)
            GROUP BY p.OfficialID, a.Category;";
        return (await conn.QueryAsync<MonthCategoryCount>(sql, new { MonthStart = monthStart })).ToList();
    }

    // Months (yyyy-MM, newest first) that still have undecided lines.
    public async Task<IReadOnlyList<string>> GetOpenMonthsAsync()
    {
        using var conn = _db.CreateConnection();
        const string sql = @"
            SELECT DISTINCT FORMAT(a.Activity_Date, 'yyyy-MM')
            FROM ActivityParticipants p
            INNER JOIN Activities a ON a.Activity_ID = p.Activity_ID
            WHERE p.Decision IS NULL
            ORDER BY 1 DESC;";
        return (await conn.QueryAsync<string>(sql)).ToList();
    }

    public (List<(OpenActivityLine Line, LineDecisionInput Decision)> Pay, List<(OpenActivityLine Line, LineDecisionInput Decision)> NotPayable, List<string> Errors)
        ValidateDecisions(IEnumerable<LineDecisionInput> decisions, IReadOnlyList<OpenActivityLine> openLines, IReadOnlyDictionary<string, string> budgetLabels, string decidedBy)
    {
        var open = openLines.ToDictionary(l => (l.Activity_ID, l.OfficialID));
        var pay = new List<(OpenActivityLine, LineDecisionInput)>();
        var no = new List<(OpenActivityLine, LineDecisionInput)>();
        var errors = new List<string>();

        foreach (var d in decisions.Where(d => !string.IsNullOrWhiteSpace(d.Decision)))
        {
            if (!open.TryGetValue((d.Activity_ID, d.OfficialID), out var line))
            {
                errors.Add($"{d.Activity_ID} / {d.OfficialID}: this line is no longer open (someone may have decided it already).");
                continue;
            }

            var label = $"{line.FullName} – {line.Title}";

            if (line.OfficialID == decidedBy)
            {
                errors.Add($"{label}: you cannot decide your own line; the other treasury officer must.");
                continue;
            }

            if (d.Decision == "Pay")
            {
                if (string.IsNullOrWhiteSpace(d.Budget_Link) || !budgetLabels.ContainsKey(d.Budget_Link))
                    errors.Add($"{label}: choose a budget item.");
                else if (d.Amount is null or <= 0)
                    errors.Add($"{label}: enter an amount greater than zero.");
                else
                    pay.Add((line, d));
            }
            else if (d.Decision == "NotPayable")
            {
                if (string.IsNullOrWhiteSpace(d.Reason))
                    errors.Add($"{label}: give the policy reason it is not payable.");
                else if (d.Reason.Trim().Length > 500)
                    errors.Add($"{label}: the reason must be 500 characters or fewer.");
                else
                    no.Add((line, d));
            }
            else
            {
                errors.Add($"{label}: unknown decision.");
            }
        }

        return (pay, no, errors);
    }

    // One voucher per official per budget item - a real voucher carries a
    // single budget item, so an official's lines that belong to different
    // items are split into separate vouchers.
    public List<VoucherPlan> PlanVouchers(IEnumerable<(OpenActivityLine Line, LineDecisionInput Decision)> pay, IReadOnlyDictionary<string, string> budgetLabels)
    {
        return pay
            .GroupBy(p => (p.Line.OfficialID, Budget: p.Decision.Budget_Link!))
            .Select(g =>
            {
                var lines = g.Select(x => x.Line).ToList();
                var (text, shortened) = ActivityDescription.Build(lines);
                return new VoucherPlan
                {
                    OfficialID = g.Key.OfficialID,
                    OfficialName = lines[0].FullName,
                    OfficialRole = lines[0].Role,
                    Budget_Link = g.Key.Budget,
                    BudgetLabel = budgetLabels[g.Key.Budget],
                    Amount = g.Sum(x => x.Decision.Amount!.Value),
                    Description = text,
                    DescriptionShortened = shortened,
                    Lines = lines
                };
            })
            .OrderBy(v => v.OfficialName).ThenBy(v => v.BudgetLabel)
            .ToList();
    }

    // All-or-nothing: the vouchers, the pay decisions and the not-payable
    // decisions are one transaction. Each UPDATE only matches a line that is
    // still undecided, so two treasury officers acting at once cannot both
    // decide (or double-pay) the same line.
    public async Task<DecisionResult> DecideAsync(IReadOnlyList<LineDecisionInput> decisions, string decidedBy, DateTime voucherDate, IReadOnlyDictionary<string, string> budgetLabels)
    {
        using var conn = (SqlConnection)_db.CreateConnection();
        conn.Open();
        using var transaction = conn.BeginTransaction();

        try
        {
            var open = (await conn.QueryAsync<OpenActivityLine>(@"
                SELECT p.Activity_ID, p.OfficialID, o.FullName, o.Role, a.Activity_Date, a.Category, a.Title, a.Days_Spent
                FROM ActivityParticipants p
                INNER JOIN Activities a ON a.Activity_ID = p.Activity_ID
                INNER JOIN Ref_Officials o ON o.OfficialID = p.OfficialID
                WHERE p.Decision IS NULL;", transaction: transaction)).ToList();

            var (pay, notPayable, errors) = ValidateDecisions(decisions, open, budgetLabels, decidedBy);
            if (errors.Count > 0)
                throw new InvalidOperationException(string.Join(" ", errors));
            if (pay.Count == 0 && notPayable.Count == 0)
                throw new InvalidOperationException("There is nothing to save: decide at least one line.");

            var result = new DecisionResult();

            const string insertVoucher = @"
                INSERT INTO Vouchers
                    (VoucherDate, Transaction_Type, Budget_Link, Official_Link, Amount,
                     [Description], Payee_Category, Is_Travel_Expense)
                OUTPUT INSERTED.Voucher_ID
                VALUES
                    (@VoucherDate, 'Expense', @BudgetLink, @OfficialId, @Amount,
                     @Description, 'Official', 0);";

            const string markPay = @"
                UPDATE ActivityParticipants
                SET Decision = 'Pay', Budget_Link = @BudgetLink, Amount = @Amount, Voucher_ID = @VoucherId,
                    Decided_By = @DecidedBy, Decided_At = GETDATE()
                WHERE Activity_ID = @ActivityId AND OfficialID = @OfficialId AND Decision IS NULL;";

            var payByLine = pay.ToDictionary(p => (p.Line.Activity_ID, p.Line.OfficialID), p => p.Decision);

            foreach (var plan in PlanVouchers(pay, budgetLabels))
            {
                var voucherId = await conn.ExecuteScalarAsync<string>(insertVoucher, new
                {
                    VoucherDate = voucherDate.Date,
                    BudgetLink = plan.Budget_Link,
                    OfficialId = plan.OfficialID,
                    plan.Amount,
                    plan.Description
                }, transaction);
                result.VoucherIds.Add(voucherId!);

                foreach (var line in plan.Lines)
                {
                    var d = payByLine[(line.Activity_ID, line.OfficialID)];
                    var rows = await conn.ExecuteAsync(markPay, new
                    {
                        BudgetLink = plan.Budget_Link,
                        Amount = d.Amount,
                        VoucherId = voucherId,
                        DecidedBy = decidedBy,
                        ActivityId = line.Activity_ID,
                        OfficialId = line.OfficialID
                    }, transaction);
                    if (rows != 1)
                        throw new InvalidOperationException($"{line.FullName} – {line.Title} was decided by someone else in the meantime. Nothing was saved; reload and try again.");
                }
            }

            const string markNo = @"
                UPDATE ActivityParticipants
                SET Decision = 'NotPayable', Reason = @Reason, Decided_By = @DecidedBy, Decided_At = GETDATE()
                WHERE Activity_ID = @ActivityId AND OfficialID = @OfficialId AND Decision IS NULL;";

            foreach (var (line, d) in notPayable)
            {
                var rows = await conn.ExecuteAsync(markNo, new
                {
                    Reason = d.Reason!.Trim(),
                    DecidedBy = decidedBy,
                    ActivityId = line.Activity_ID,
                    OfficialId = line.OfficialID
                }, transaction);
                if (rows != 1)
                    throw new InvalidOperationException($"{line.FullName} – {line.Title} was decided by someone else in the meantime. Nothing was saved; reload and try again.");
                result.NotPayableCount++;
            }

            transaction.Commit();
            return result;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    // Checked against the real bytes, not the extension or declared type.
    private static bool TryDetectContentType(byte[] data, out string? contentType)
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

        if (data.Length >= 5 && data[0] == 0x25 && data[1] == 0x50 && data[2] == 0x44 && data[3] == 0x46 && data[4] == 0x2D)
        {
            contentType = "application/pdf";
            return true;
        }

        contentType = null;
        return false;
    }
}
