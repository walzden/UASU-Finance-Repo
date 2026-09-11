using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UASU_VoucherApprovals.Models;
using UASU_VoucherApprovals.Services;

namespace UASU_VoucherApprovals.Pages.Reports;

// [Authorize] only, read-only - same as every other report. The
// year-end RTU submission: Members and Agency Payers as two separate
// lists (mirrors the physical register), each row showing every month
// that contributor paid something and their yearly total.
[Authorize]
public class MemberRegisterModel : PageModel
{
    private readonly IMemberRegisterService _memberRegisterService;

    public MemberRegisterModel(IMemberRegisterService memberRegisterService)
    {
        _memberRegisterService = memberRegisterService;
    }

    [BindProperty(SupportsGet = true, Name = "year")]
    public int SelectedYear { get; set; }

    // "financial" shows each month's actual amount plus a Total column
    // and a TOTALS footer row; "simple" is just presence/absence
    // (checkmark/dash) with no totals at all - two different audiences
    // for the same underlying data (an auditor wanting figures vs. RTU
    // just wanting to see who paid which months).
    [BindProperty(SupportsGet = true, Name = "view")]
    public string ViewMode { get; set; } = "financial";

    public List<int> AvailableYears { get; set; } = new();
    public List<MemberRegisterRow> Members { get; set; } = new();
    public List<MemberRegisterRow> AgencyPayers { get; set; } = new();

    // Uppercase to match the physical UASU Membership Register ledger's
    // own column headers exactly (JAN, FEB, ... not Jan, Feb).
    public static readonly string[] MonthAbbrev =
        { "JAN", "FEB", "MAR", "APR", "MAY", "JUN", "JUL", "AUG", "SEP", "OCT", "NOV", "DEC" };

    public async Task OnGetAsync()
    {
        AvailableYears = (await _memberRegisterService.GetAvailableYearsAsync()).ToList();
        if (SelectedYear == 0)
            SelectedYear = AvailableYears.FirstOrDefault(y => y == DateTime.Today.Year, AvailableYears.FirstOrDefault());

        Members = (await _memberRegisterService.GetAnnualRegisterAsync(SelectedYear, "Member")).ToList();
        AgencyPayers = (await _memberRegisterService.GetAnnualRegisterAsync(SelectedYear, "Agency")).ToList();
    }
}
