using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using UASU_VoucherApprovals.Data;

namespace UASU_VoucherApprovals.Pages.Account;

// Onboarding tool for officials generally, not a one-off script and not
// limited to the four officer roles - safe to re-run any time a new
// person joins Ref_Officials. It never touches an account that already
// exists (see the "exists is null" check), so re-running this after a
// new person is added won't reset any password already in active use.
//
// Seeding a login for a role outside Chairman/Chapter Secretary/
// Treasurer/Deputy Treasurer is safe by construction, not just by
// convention: the Approvers and TreasuryAdmin policies both key off
// RequireRole with those four exact strings, so any other role only
// ever satisfies the bare [Authorize] pages (Reports, the dashboard on
// Index) - never approval, never anything TreasuryAdmin-gated.
[Authorize(Policy = "TreasuryAdmin")]
public class SeedDefaultLoginsModel : PageModel
{
    private readonly IDbConnectionFactory _db;
    private const string DefaultPassword = "test1234";

    public SeedDefaultLoginsModel(IDbConnectionFactory db)
    {
        _db = db;
    }

    public List<string> Results { get; set; } = new();

    public void OnGet() { }

    public async Task OnPostAsync()
    {
        // Hashed once in-process and reused - no typing, no copy-paste,
        // no chance of the mismatch that bit us with the manual approach.
        var hash = BCrypt.Net.BCrypt.HashPassword(DefaultPassword);
        var selfCheck = BCrypt.Net.BCrypt.Verify(DefaultPassword, hash);
        Results.Add($"Hash generated for \"{DefaultPassword}\", self-verify: {selfCheck}");

        if (!selfCheck)
        {
            Results.Add("STOPPING - BCrypt failed to verify its own output.");
            return;
        }

        using var conn = _db.CreateConnection();

        const string officialsSql = @"
            SELECT OfficialID, FullName, Role, Email
            FROM Ref_Officials
            WHERE IsCurrent = 1
            ORDER BY FullName;";

        var officials = (await conn.QueryAsync(officialsSql)).ToList();

        Results.Add("");
        Results.Add($"Found {officials.Count} current official(s) in Ref_Officials:");

        foreach (var o in officials)
        {
            string officialId = o.OfficialID;
            string fullName = o.FullName;
            string role = o.Role ?? "(no role on file)";
            string? email = o.Email;

            if (string.IsNullOrWhiteSpace(email))
            {
                Results.Add($"  SKIPPED - {fullName} ({role}): no email on file in Ref_Officials to use as a username.");
                continue;
            }

            var exists = await conn.QuerySingleOrDefaultAsync<string>(
                "SELECT OfficialID FROM Users WHERE OfficialID = @OfficialID;", new { OfficialID = officialId });

            if (exists is null)
            {
                try
                {
                    await conn.ExecuteAsync(
                        @"INSERT INTO Users (OfficialID, Username, PasswordHash, MustChangePassword)
                          VALUES (@OfficialID, @Email, @Hash, 1);",
                        new { OfficialID = officialId, Email = email, Hash = hash });
                    Results.Add($"  CREATED - {fullName} ({role}): username = {email}");
                }
                catch (SqlException ex) when (ex.Number is 2627 or 2601)
                {
                    // Someone else - another concurrent click of this same
                    // button, or a second officer running it at the same
                    // moment - created this login in the instant between
                    // our check above and this insert. That's a benign
                    // race, not a real failure, so treat it the same as
                    // finding it already existed rather than crashing the
                    // whole page and leaving every official after this one
                    // in the list unprocessed.
                    Results.Add($"  SKIPPED - {fullName} ({role}): already has a login (created concurrently), left untouched.");
                }
            }
            else
            {
                // Deliberately does NOT reset an existing account - this
                // page is meant to be re-run over time as new officers
                // join, and must never disturb someone who's already
                // signed in and changed their own password.
                Results.Add($"  SKIPPED - {fullName} ({role}): already has a login, left untouched.");
            }
        }

        Results.Add("");
        Results.Add($"Done. Anyone listed as CREATED can now log in with their email and password \"{DefaultPassword}\", and will be forced to change it immediately. Only Chairman/Chapter Secretary/Treasurer/Deputy Treasurer get anything beyond read-only access - everyone else lands on the dashboard and reports.");
    }
}
