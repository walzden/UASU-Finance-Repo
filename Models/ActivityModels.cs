using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace UASU_VoucherApprovals.Models;

// Fixed pick-lists for the activity log. Kept in code (not tables) for now;
// PolicyReasons in particular is placeholder wording until the treasury
// supplies the real financial-policy text.
public static class ActivityLists
{
    public static readonly string[] Categories =
    {
        "Union meeting / committee",
        "Representation (external body)",
        "Travel on union business",
        "Negotiation / engagement with management",
        "Member welfare visit",
        "Secretariat / admin work",
        "Other"
    };

    public static readonly string[] AuthorisedBy =
    {
        "Chairman",
        "Chapter Secretary",
        "Executive meeting resolution",
        "UASU Constitution",
        "Self-initiated (to be confirmed)"
    };

    public static readonly string[] PolicyReasons =
    {
        "Monthly cap reached: only 4 committee meetings a month are payable (sample policy wording).",
        "Covered by the standing honorarium; not paid separately (sample policy wording).",
        "Not authorised by the Chairman, Chapter Secretary or an executive resolution (sample policy wording).",
        "Insufficient evidence attached to support the claim (sample policy wording)."
    };
}

// Form input for logging one activity. ParticipantIds is everyone who took
// part - the logger need not be one of them (a colleague can record for
// officials who have not signed in yet).
public class ActivityInputModel
{
    [Required]
    [DataType(DataType.Date)]
    public DateTime Activity_Date { get; set; } = DateTime.Today;

    [Required]
    public string Category { get; set; } = ActivityLists.Categories[0];

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(2000)]
    public string Details { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Place { get; set; }

    [Required]
    [Range(0.5, 31, ErrorMessage = "Days spent must be between 0.5 and 31.")]
    public decimal Days_Spent { get; set; } = 1;

    [MaxLength(300)]
    public string? Others_Involved { get; set; }

    [Required]
    public string Authorised_By { get; set; } = ActivityLists.AuthorisedBy[0];

    [Range(0, 10000000, ErrorMessage = "Expenses cannot be negative.")]
    public decimal Expenses_Amount { get; set; }

    [MaxLength(300)]
    public string? Expenses_Note { get; set; }

    public List<string> ParticipantIds { get; set; } = new();

    public List<IFormFile> Evidence { get; set; } = new();
}

// One official on the participant picker.
public class ActivityOfficialOption
{
    public string OfficialID { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Role { get; set; }
    // True until they have signed in once - colleagues record their
    // activities for them until then.
    public bool NotSignedIn { get; set; }
}

public class ActivityEvidenceInfo
{
    public int Evidence_ID { get; set; }
    public string Activity_ID { get; set; } = string.Empty;
    public string File_Name { get; set; } = string.Empty;
}

public class ActivityEvidenceFile
{
    public byte[] File_Data { get; set; } = Array.Empty<byte>();
    public string File_Name { get; set; } = string.Empty;
    public string Content_Type { get; set; } = string.Empty;
}

// One official's line on an activity, as shown on the activity history.
public class ActivityLineView
{
    public string Activity_ID { get; set; } = string.Empty;
    public string OfficialID { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public bool NotSignedIn { get; set; }
    public string? Decision { get; set; }
    public string? Reason { get; set; }
    // A Pay decision creates a debt owed to the official; the treasury later
    // raises a voucher from it. Voucher_ID is that voucher (or, for lines
    // decided before debts existed, the voucher created directly).
    public string? Debt_ID { get; set; }
    public decimal? DebtAmount { get; set; }
    public string? Voucher_ID { get; set; }
    public string? VoucherStatus { get; set; }
}

public class ActivityView
{
    public string Activity_ID { get; set; } = string.Empty;
    public DateTime Activity_Date { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public string? Place { get; set; }
    public decimal Days_Spent { get; set; }
    public string? Others_Involved { get; set; }
    public string Authorised_By { get; set; } = string.Empty;
    public decimal Expenses_Amount { get; set; }
    public string? Expenses_Note { get; set; }
    public string LoggedByName { get; set; } = string.Empty;
    public string Logged_By { get; set; } = string.Empty;
    public List<ActivityLineView> Lines { get; set; } = new();
    public List<ActivityEvidenceInfo> Evidence { get; set; } = new();
}

// One undecided line on the treasury's Activity Decisions page.
public class OpenActivityLine
{
    public string Activity_ID { get; set; } = string.Empty;
    public string OfficialID { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Role { get; set; }
    public bool NotSignedIn { get; set; }
    public DateTime Activity_Date { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public decimal Days_Spent { get; set; }
    public int ParticipantCount { get; set; }
    public decimal Expenses_Amount { get; set; }
    public string? Expenses_Note { get; set; }
    public string LoggedByName { get; set; } = string.Empty;
}

// How many activities of each type an official has in the month (any
// status), so the treasury can apply caps like "max 4 committee meetings".
public class MonthCategoryCount
{
    public string OfficialID { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int Total { get; set; }
    public int Paid { get; set; }
}

// The treasury's decision on one line, posted from the Decide page.
public class LineDecisionInput
{
    public string Activity_ID { get; set; } = string.Empty;
    public string OfficialID { get; set; } = string.Empty;
    public string? Decision { get; set; }      // "Pay" | "NotPayable" | blank (leave undecided)
    public decimal? Amount { get; set; }        // the amount owed, for Pay
    public string? Reason { get; set; }
}

public class DecisionResult
{
    public List<string> DebtIds { get; set; } = new();
    public int NotPayableCount { get; set; }
}
