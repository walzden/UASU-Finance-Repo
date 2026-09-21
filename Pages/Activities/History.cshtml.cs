using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UASU_VoucherApprovals.Models;
using UASU_VoucherApprovals.Services;

namespace UASU_VoucherApprovals.Pages.Activities;

// [Authorize] only: every signed-in official sees what they have logged or
// taken part in, and what the treasury decided for each of their lines.
[Authorize]
public class HistoryModel : PageModel
{
    private readonly IActivityService _activityService;

    public HistoryModel(IActivityService activityService)
    {
        _activityService = activityService;
    }

    // all | waiting | owed | paid | no
    [BindProperty(SupportsGet = true)]
    public string? Filter { get; set; }

    public IReadOnlyList<ActivityView> Shown { get; set; } = Array.Empty<ActivityView>();
    public Dictionary<string, int> Counts { get; set; } = new() { ["all"] = 0, ["waiting"] = 0, ["owed"] = 0, ["paid"] = 0, ["no"] = 0 };

    // The official's own lines (not lines they merely recorded for others).
    public int MyWaiting { get; set; }
    public int MyOwed { get; set; }
    public decimal MyOwedAmount { get; set; }
    public int MyPaid { get; set; }
    public int MyNotPayable { get; set; }

    public string CurrentOfficialId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    public async Task OnGetAsync()
    {
        var all = await _activityService.GetMyActivitiesAsync(CurrentOfficialId);

        Filter = Filter is "waiting" or "owed" or "paid" or "no" ? Filter : "all";

        foreach (var a in all)
        {
            var states = MyLines(a).Select(State).ToList();
            Counts["all"]++;
            foreach (var s in states.Distinct())
                Counts[s]++;

            var mine = a.Lines.FirstOrDefault(l => l.OfficialID == CurrentOfficialId);
            if (mine is not null)
            {
                switch (State(mine))
                {
                    case "paid": MyPaid++; break;
                    case "owed": MyOwed++; MyOwedAmount += mine.DebtAmount ?? 0; break;
                    case "no": MyNotPayable++; break;
                    default: MyWaiting++; break;
                }
            }
        }

        Shown = Filter == "all"
            ? all
            : all.Where(a => MyLines(a).Any(l => State(l) == Filter)).ToList();
    }

    // The lines that describe "my" state on an activity: my own line if I
    // took part, otherwise every line (I only recorded it for others).
    public IEnumerable<ActivityLineView> MyLines(ActivityView a) =>
        a.Lines.Any(l => l.OfficialID == CurrentOfficialId)
            ? a.Lines.Where(l => l.OfficialID == CurrentOfficialId)
            : a.Lines;

    // waiting = undecided; owed = decided Pay, debt recorded, no voucher yet;
    // paid = on a voucher; no = not payable.
    public static string State(ActivityLineView l) =>
        l.Voucher_ID is not null ? "paid"
        : l.Decision == "NotPayable" ? "no"
        : l.Decision == "Pay" ? "owed"
        : "waiting";
}
