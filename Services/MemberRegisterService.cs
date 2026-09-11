using Microsoft.Data.SqlClient;
using Dapper;
using UASU_VoucherApprovals.Data;
using UASU_VoucherApprovals.Models;

namespace UASU_VoucherApprovals.Services;

// Backs the RTU annual member register (SQL/026) - Union_Contributors is
// the permanent roster (keyed by a generated Contributor_ID, separate
// from MMU's own PF_No), Union_Registrations is just the (contributor,
// year) pairing, and Union_Contributions is one row per registration per
// month, carrying that month's own Member/Agency status - a person can
// switch mid-year, so the type lives at the monthly grain, not the
// yearly one. See SQL/026 for the full reasoning.
public interface IMemberRegisterService
{
    Task<IEnumerable<ContributorRow>> GetContributorsAsync();
    Task<string> CreateContributorAsync(ContributorInputModel input);
    Task<ContributorInputModel?> GetContributorForEditAsync(string contributorId);
    Task UpdateContributorAsync(string contributorId, ContributorInputModel input);

    Task<IEnumerable<int>> GetAvailableYearsAsync();
    Task<IEnumerable<ContributionEntryRow>> GetContributionEntryGridAsync(int year, int month);
    Task SaveContributionsAsync(int year, int month, IEnumerable<ContributionEntryInput> entries);

    Task<IEnumerable<MemberRegisterRow>> GetAnnualRegisterAsync(int year, string membershipType);
}

public class MemberRegisterService : IMemberRegisterService
{
    private readonly IDbConnectionFactory _db;

    public MemberRegisterService(IDbConnectionFactory db)
    {
        _db = db;
    }

    public async Task<IEnumerable<ContributorRow>> GetContributorsAsync()
    {
        using var conn = _db.CreateConnection();
        const string sql = @"
            SELECT Contributor_ID, PF_No, Full_Name, Created_Date
            FROM Union_Contributors
            ORDER BY Full_Name;";
        return await conn.QueryAsync<ContributorRow>(sql);
    }

    public async Task<string> CreateContributorAsync(ContributorInputModel input)
    {
        using var conn = _db.CreateConnection();
        const string sql = @"
            INSERT INTO Union_Contributors (PF_No, Full_Name)
            OUTPUT INSERTED.Contributor_ID
            VALUES (@PF_No, @Full_Name);";
        return await conn.ExecuteScalarAsync<string>(sql, input) ?? string.Empty;
    }

    public async Task<ContributorInputModel?> GetContributorForEditAsync(string contributorId)
    {
        using var conn = _db.CreateConnection();
        const string sql = "SELECT PF_No, Full_Name FROM Union_Contributors WHERE Contributor_ID = @ContributorId;";
        return await conn.QuerySingleOrDefaultAsync<ContributorInputModel>(sql, new { ContributorId = contributorId });
    }

    public async Task UpdateContributorAsync(string contributorId, ContributorInputModel input)
    {
        using var conn = _db.CreateConnection();
        const string sql = @"
            UPDATE Union_Contributors
            SET PF_No = @PF_No, Full_Name = @Full_Name
            WHERE Contributor_ID = @ContributorId;";
        await conn.ExecuteAsync(sql, new { ContributorId = contributorId, input.PF_No, input.Full_Name });
    }

    public async Task<IEnumerable<int>> GetAvailableYearsAsync()
    {
        using var conn = _db.CreateConnection();
        const string sql = "SELECT DISTINCT Register_Year FROM Union_Registrations ORDER BY Register_Year DESC;";
        return await conn.QueryAsync<int>(sql);
    }

    // Every contributor, left-joined against whatever this (Year, Month)
    // already has saved for them - Membership_Type defaults to 'Member'
    // when nothing's saved for this month yet, since that's the more
    // common case and the officer can switch it per row. It comes from
    // Union_Contributions (per-month), not Union_Registrations, since a
    // person's status can change mid-year - see SQL/026.
    public async Task<IEnumerable<ContributionEntryRow>> GetContributionEntryGridAsync(int year, int month)
    {
        using var conn = _db.CreateConnection();
        const string sql = @"
            SELECT c.Contributor_ID, c.PF_No, c.Full_Name,
                   ISNULL(con.Membership_Type, 'Member') AS Membership_Type,
                   con.Amount
            FROM Union_Contributors c
            LEFT JOIN Union_Registrations r ON r.Contributor_ID = c.Contributor_ID AND r.Register_Year = @Year
            LEFT JOIN Union_Contributions con ON con.Registration_ID = r.Registration_ID AND con.Contribution_Month = @Month
            ORDER BY c.Full_Name;";
        return await conn.QueryAsync<ContributionEntryRow>(sql, new { Year = year, Month = month });
    }

    // A row with no Amount is skipped entirely rather than creating an
    // empty registration for it - a contributor only becomes "registered"
    // for a year once they actually have a contribution in it. A row
    // whose Amount was cleared (now null but previously saved) still
    // needs its old Union_Contributions row removed, which the delete
    // branch below handles even when no registration change is needed.
    public async Task SaveContributionsAsync(int year, int month, IEnumerable<ContributionEntryInput> entries)
    {
        using var conn = (SqlConnection)_db.CreateConnection();
        conn.Open();
        using var transaction = conn.BeginTransaction();

        try
        {
            // No Membership_Type here - Union_Registrations is now just
            // the (contributor, year) pairing, so a matched row needs no
            // update at all. OUTPUT still needs a WHEN MATCHED branch to
            // fire on an already-registered contributor, so it's a no-op
            // update rather than an omitted clause.
            const string mergeRegistration = @"
                MERGE Union_Registrations AS target
                USING (SELECT @Contributor_ID AS Contributor_ID, @Year AS Register_Year) AS src
                ON target.Contributor_ID = src.Contributor_ID AND target.Register_Year = src.Register_Year
                WHEN MATCHED THEN UPDATE SET Register_Year = src.Register_Year
                WHEN NOT MATCHED THEN
                    INSERT (Contributor_ID, Register_Year)
                    VALUES (@Contributor_ID, @Year)
                OUTPUT INSERTED.Registration_ID;";

            const string mergeContribution = @"
                MERGE Union_Contributions AS target
                USING (SELECT @Registration_ID AS Registration_ID, @Month AS Contribution_Month) AS src
                ON target.Registration_ID = src.Registration_ID AND target.Contribution_Month = src.Contribution_Month
                WHEN MATCHED THEN UPDATE SET Amount = @Amount, Membership_Type = @Membership_Type
                WHEN NOT MATCHED THEN
                    INSERT (Registration_ID, Contribution_Month, Membership_Type, Amount)
                    VALUES (@Registration_ID, @Month, @Membership_Type, @Amount);";

            const string deleteContribution = @"
                DELETE con FROM Union_Contributions con
                INNER JOIN Union_Registrations r ON r.Registration_ID = con.Registration_ID
                WHERE r.Contributor_ID = @Contributor_ID AND r.Register_Year = @Year AND con.Contribution_Month = @Month;";

            foreach (var entry in entries)
            {
                if (entry.Amount is > 0)
                {
                    var registrationId = await conn.ExecuteScalarAsync<int>(mergeRegistration,
                        new { entry.Contributor_ID, Year = year }, transaction);
                    await conn.ExecuteAsync(mergeContribution,
                        new { Registration_ID = registrationId, Month = month, entry.Amount }, transaction);
                }
                else
                {
                    await conn.ExecuteAsync(deleteContribution,
                        new { entry.Contributor_ID, Year = year, Month = month }, transaction);
                }
            }

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    // Flat (contributor, month, amount) rows, grouped into one register
    // row per contributor in C# - same pivot-in-code approach ReportService
    // already uses for Cash Book, rather than a 12-column SQL pivot.
    // Filters on the CONTRIBUTION's Membership_Type, not the year-level
    // registration - someone who switched from Agency to Member mid-year
    // shows up on both registers, each with only their matching months
    // filled in, exactly like the two source Excel sheets already do.
    public async Task<IEnumerable<MemberRegisterRow>> GetAnnualRegisterAsync(int year, string membershipType)
    {
        using var conn = _db.CreateConnection();
        const string sql = @"
            SELECT c.Contributor_ID, c.PF_No, c.Full_Name, con.Membership_Type,
                   con.Contribution_Month, con.Amount
            FROM Union_Registrations r
            INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
            INNER JOIN Union_Contributions con ON con.Registration_ID = r.Registration_ID
            WHERE r.Register_Year = @Year AND con.Membership_Type = @MembershipType
            ORDER BY TRY_CAST(c.PF_No AS INT);";

        var flat = await conn.QueryAsync<(string Contributor_ID, string PF_No, string Full_Name, string Membership_Type, int? Contribution_Month, decimal? Amount)>(
            sql, new { Year = year, MembershipType = membershipType });

        var rows = new List<MemberRegisterRow>();
        foreach (var group in flat.GroupBy(f => f.Contributor_ID))
        {
            var first = group.First();
            var row = new MemberRegisterRow
            {
                Contributor_ID = first.Contributor_ID,
                PF_No = first.PF_No,
                Full_Name = first.Full_Name,
                Membership_Type = first.Membership_Type
            };
            foreach (var entry in group.Where(e => e.Contribution_Month is not null))
                row.MonthlyAmounts[entry.Contribution_Month!.Value - 1] = entry.Amount;
            rows.Add(row);
        }
        return rows;
    }
}
