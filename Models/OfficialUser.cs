namespace UASU_VoucherApprovals.Models;

// Result of a successful login: enough info to build the auth cookie's
// claims (identity + role) without re-querying the database on every page.
public class OfficialUser
{
    public string OfficialID { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public bool MustChangePassword { get; set; }
    public bool EmailVerified { get; set; }
}

// Row shape returned by the credential check query (includes the hash,
// which OfficialUser deliberately does not carry past the auth check).
public class UserCredentialRecord
{
    public string OfficialID { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool MustChangePassword { get; set; }
    public bool EmailVerified { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}
