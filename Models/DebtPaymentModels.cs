namespace UASU_VoucherApprovals.Models;

// One outstanding debt owed to an official that the treasury can raise a
// voucher for on the Pay Debts page. Debts created from activity decisions
// carry their activity; older/manual debts do not.
public class DebtToPay
{
    public string Debt_ID { get; set; } = string.Empty;
    public string OfficialID { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Role { get; set; }
    public bool NotSignedIn { get; set; }
    public DateTime Date_Incurred { get; set; }
    public string? Description { get; set; }
    public decimal Total_Owed { get; set; }

    public string? Activity_ID { get; set; }
    public string? Category { get; set; }
    public string? DecidedByName { get; set; }

    public int DaysOld => (DateTime.Today - Date_Incurred.Date).Days;
}

// One row of the Pay Debts form: which debt, whether it is ticked, and the
// budget item the voucher for it will carry.
public class DebtSelectionInput
{
    public string Debt_ID { get; set; } = string.Empty;
    public bool Selected { get; set; }
    public string? Budget_Link { get; set; }
}

// One voucher the treasury is about to raise: every ticked debt of one
// official under one budget item (a voucher carries a single budget item).
public class VoucherPlan
{
    public string OfficialID { get; set; } = string.Empty;
    public string OfficialName { get; set; } = string.Empty;
    public string? OfficialRole { get; set; }
    public string Budget_Link { get; set; } = string.Empty;
    public string BudgetLabel { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool DescriptionShortened { get; set; }
    public List<DebtToPay> Debts { get; set; } = new();
}
