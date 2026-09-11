using Dapper;
using UASU_VoucherApprovals.Data;
using UASU_VoucherApprovals.Models;

namespace UASU_VoucherApprovals.Services;

public interface IAuthService
{
    Task<OfficialUser?> ValidateCredentialsAsync(string username, string password);
    Task RecordLoginAsync(string officialId);
    Task<bool> ChangePasswordAsync(string officialId, string currentPassword, string newPassword);

    Task<bool> IsEmailVerifiedAsync(string officialId);
    Task<bool> HasPendingVerificationCodeAsync(string officialId);
    Task<string?> GetVerificationEmailAsync(string officialId);
    Task<string> IssueVerificationCodeAsync(string officialId);
    Task<bool> ConfirmVerificationCodeAsync(string officialId, string code);

    Task<string?> GetOfficialIdByUsernameAsync(string username);
    Task ResetPasswordAsync(string officialId, string newPassword);
}

public class AuthService : IAuthService
{
    private readonly IDbConnectionFactory _db;

    public AuthService(IDbConnectionFactory db)
    {
        _db = db;
    }

    public async Task<OfficialUser?> ValidateCredentialsAsync(string username, string password)
    {
        using var conn = _db.CreateConnection();

        // Join Users -> Ref_Officials so a role change or a former
        // officer being marked IsCurrent = 0 takes effect on next login
        // without touching the Users table at all.
        const string sql = @"
            SELECT u.OfficialID, u.Username, u.PasswordHash, u.IsActive, u.MustChangePassword, u.EmailVerified,
                   o.FullName, o.Role
            FROM Users u
            INNER JOIN Ref_Officials o ON o.OfficialID = u.OfficialID
            WHERE u.Username = @Username AND o.IsCurrent = 1;";

        var record = await conn.QuerySingleOrDefaultAsync<UserCredentialRecord>(sql, new { Username = username });

        if (record is null || !record.IsActive)
            return null;

        if (!BCrypt.Net.BCrypt.Verify(password, record.PasswordHash))
            return null;

        return new OfficialUser
        {
            OfficialID = record.OfficialID,
            FullName = record.FullName,
            Role = record.Role,
            Username = record.Username,
            MustChangePassword = record.MustChangePassword,
            EmailVerified = record.EmailVerified
        };
    }

    public async Task RecordLoginAsync(string officialId)
    {
        using var conn = _db.CreateConnection();
        const string sql = "UPDATE Users SET LastLogin = GETDATE() WHERE OfficialID = @OfficialID;";
        await conn.ExecuteAsync(sql, new { OfficialID = officialId });
    }

    public async Task<bool> ChangePasswordAsync(string officialId, string currentPassword, string newPassword)
    {
        using var conn = _db.CreateConnection();

        const string lookupSql = "SELECT PasswordHash FROM Users WHERE OfficialID = @OfficialID;";
        var currentHash = await conn.QuerySingleOrDefaultAsync<string>(lookupSql, new { OfficialID = officialId });

        if (currentHash is null || !BCrypt.Net.BCrypt.Verify(currentPassword, currentHash))
            return false;

        var newHash = BCrypt.Net.BCrypt.HashPassword(newPassword);

        const string updateSql = @"
            UPDATE Users
            SET PasswordHash = @NewHash, MustChangePassword = 0
            WHERE OfficialID = @OfficialID;";

        await conn.ExecuteAsync(updateSql, new { NewHash = newHash, OfficialID = officialId });
        return true;
    }

    // ------------------------------------------------------------------
    // Email verification (first-login gate, SQL/025)
    // ------------------------------------------------------------------

    public async Task<bool> IsEmailVerifiedAsync(string officialId)
    {
        using var conn = _db.CreateConnection();
        const string sql = "SELECT EmailVerified FROM Users WHERE OfficialID = @OfficialId;";
        return await conn.ExecuteScalarAsync<bool>(sql, new { OfficialId = officialId });
    }

    public async Task<bool> HasPendingVerificationCodeAsync(string officialId)
    {
        using var conn = _db.CreateConnection();
        const string sql = @"
            SELECT CASE WHEN VerificationCodeExpiry IS NOT NULL AND VerificationCodeExpiry > GETDATE() THEN 1 ELSE 0 END
            FROM Users WHERE OfficialID = @OfficialId;";
        return await conn.ExecuteScalarAsync<bool>(sql, new { OfficialId = officialId });
    }

    // Username doubles as the officer's email address for every login
    // created by SeedDefaultLogins/HashPasswordCli (see README) - there's
    // no separate email column on Users, so this is that same value.
    public async Task<string?> GetVerificationEmailAsync(string officialId)
    {
        using var conn = _db.CreateConnection();
        const string sql = "SELECT Username FROM Users WHERE OfficialID = @OfficialId;";
        return await conn.QuerySingleOrDefaultAsync<string>(sql, new { OfficialId = officialId });
    }

    public async Task<string> IssueVerificationCodeAsync(string officialId)
    {
        using var conn = _db.CreateConnection();

        // 6-digit numeric code, crypto-random (not Random) since it
        // gates account access the same way a password does. Hashed
        // with the same BCrypt used for passwords rather than stored
        // in plain text, even though it's short-lived.
        var code = System.Security.Cryptography.RandomNumberGenerator.GetInt32(100_000, 1_000_000).ToString();
        var hash = BCrypt.Net.BCrypt.HashPassword(code);

        const string sql = @"
            UPDATE Users
            SET VerificationCodeHash = @Hash, VerificationCodeExpiry = @Expiry
            WHERE OfficialID = @OfficialId;";
        await conn.ExecuteAsync(sql, new { Hash = hash, Expiry = DateTime.Now.AddMinutes(15), OfficialId = officialId });

        return code;
    }

    public async Task<bool> ConfirmVerificationCodeAsync(string officialId, string code)
    {
        using var conn = _db.CreateConnection();

        const string lookupSql = @"
            SELECT VerificationCodeHash, VerificationCodeExpiry
            FROM Users WHERE OfficialID = @OfficialId;";
        var record = await conn.QuerySingleOrDefaultAsync<VerificationCodeRecord>(lookupSql, new { OfficialId = officialId });

        if (record?.VerificationCodeHash is null || record.VerificationCodeExpiry is null)
            return false;
        if (record.VerificationCodeExpiry < DateTime.Now)
            return false;
        if (!BCrypt.Net.BCrypt.Verify(code, record.VerificationCodeHash))
            return false;

        const string updateSql = @"
            UPDATE Users
            SET EmailVerified = 1, VerificationCodeHash = NULL, VerificationCodeExpiry = NULL
            WHERE OfficialID = @OfficialId;";
        await conn.ExecuteAsync(updateSql, new { OfficialId = officialId });

        return true;
    }

    // ------------------------------------------------------------------
    // Forgot password (reuses the same code issue/confirm above - a
    // confirmed code proves email ownership either way, whether that's
    // "first login" or "I forgot my password")
    // ------------------------------------------------------------------

    // Only matches active logins for an official still currently serving,
    // same join/filter ValidateCredentialsAsync uses - a former officer
    // or a deactivated login can't be reset here either.
    public async Task<string?> GetOfficialIdByUsernameAsync(string username)
    {
        using var conn = _db.CreateConnection();
        const string sql = @"
            SELECT u.OfficialID
            FROM Users u
            INNER JOIN Ref_Officials o ON o.OfficialID = u.OfficialID
            WHERE u.Username = @Username AND u.IsActive = 1 AND o.IsCurrent = 1;";
        return await conn.QuerySingleOrDefaultAsync<string>(sql, new { Username = username });
    }

    // Unlike ChangePasswordAsync, takes no current password - reaching
    // this point already required confirming a code sent to the
    // account's own email (ConfirmVerificationCodeAsync), which serves
    // as the proof of identity here instead.
    public async Task ResetPasswordAsync(string officialId, string newPassword)
    {
        using var conn = _db.CreateConnection();
        var newHash = BCrypt.Net.BCrypt.HashPassword(newPassword);

        const string sql = @"
            UPDATE Users
            SET PasswordHash = @NewHash, MustChangePassword = 0
            WHERE OfficialID = @OfficialId;";
        await conn.ExecuteAsync(sql, new { NewHash = newHash, OfficialId = officialId });
    }
}
