using System.ComponentModel.DataAnnotations;

namespace UASU_VoucherApprovals.Models;

public class ContributorRow
{
    public string Contributor_ID { get; set; } = string.Empty;
    public string PF_No { get; set; } = string.Empty;
    public string Full_Name { get; set; } = string.Empty;
    public DateTime Created_Date { get; set; }
}

public class ContributorInputModel
{
    [Required, MaxLength(20)]
    [Display(Name = "PF No.")]
    public string PF_No { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    [Display(Name = "Full Name")]
    public string Full_Name { get; set; } = string.Empty;
}

// One row of the bulk month-entry grid - Membership_Type/Amount reflect
// whatever is already saved for the selected (Year, Month), if anything.
public class ContributionEntryRow
{
    public string Contributor_ID { get; set; } = string.Empty;
    public string PF_No { get; set; } = string.Empty;
    public string Full_Name { get; set; } = string.Empty;
    public string Membership_Type { get; set; } = "Member";
    public decimal? Amount { get; set; }
}

// Posted back from the grid, one per contributor row on the page.
public class ContributionEntryInput
{
    public string Contributor_ID { get; set; } = string.Empty;
    public string Membership_Type { get; set; } = "Member";
    public decimal? Amount { get; set; }
}

// One row of the printable annual register - mirrors the physical
// register's layout (PF No., Name, Jan..Dec, Total).
public class MemberRegisterRow
{
    public string Contributor_ID { get; set; } = string.Empty;
    public string PF_No { get; set; } = string.Empty;
    public string Full_Name { get; set; } = string.Empty;
    public string Membership_Type { get; set; } = string.Empty;
    public decimal?[] MonthlyAmounts { get; set; } = new decimal?[12];
    public decimal Total => MonthlyAmounts.Sum(a => a ?? 0);
    public int MonthsContributed => MonthlyAmounts.Count(a => a is > 0);
}
