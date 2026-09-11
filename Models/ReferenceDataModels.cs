using System.ComponentModel.DataAnnotations;

namespace UASU_VoucherApprovals.Models;

// Ref_Officials/Ref_Suppliers have no CHECK constraint on Role/Service_
// Category - Ref_Officials in particular holds the full leadership
// roster (delegates, committee members, etc.), not just the four roles
// with app logins, so Role stays free text here rather than a fixed
// enum. Getting the four officer role strings exactly right still
// matters, though - SeedDefaultLogins and the Approvers/TreasuryAdmin
// policies match on them exactly.
public class OfficialInputModel
{
    [Required]
    [MaxLength(150)]
    public string FullName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Role { get; set; }

    public bool IsCurrent { get; set; } = true;

    [MaxLength(100)]
    [EmailAddress]
    public string? Email { get; set; }

    [MaxLength(50)]
    public string? Phone_No { get; set; }

    [MaxLength(255)]
    public string? Address { get; set; }
}

public class OfficialRow
{
    public string OfficialID { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Role { get; set; }
    public bool IsCurrent { get; set; }
    public string? Email { get; set; }
    public string? Phone_No { get; set; }
    public string? Address { get; set; }
}

public class SupplierInputModel
{
    [Required]
    [MaxLength(150)]
    public string Business_Name { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Service_Category { get; set; }

    [MaxLength(50)]
    public string? Tax_PIN { get; set; }

    [MaxLength(50)]
    public string? Contact_Phone { get; set; }

    [MaxLength(100)]
    [EmailAddress]
    public string? Email { get; set; }

    [MaxLength(255)]
    public string? Address { get; set; }
}

public class SupplierRow
{
    public string Supplier_ID { get; set; } = string.Empty;
    public string Business_Name { get; set; } = string.Empty;
    public string? Service_Category { get; set; }
    public string? Tax_PIN { get; set; }
    public string? Contact_Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
}
