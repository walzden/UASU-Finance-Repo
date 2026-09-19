using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using UASU_VoucherApprovals.Models;
using UASU_VoucherApprovals.Services;

namespace UASU_VoucherApprovals.Pages.Activities;

// [Authorize] only, no role restriction: every signed-in official can log.
// Officials who have not signed in yet have their activities recorded by a
// colleague, so the participant list covers everyone, not just the logger.
[Authorize]
public class LogModel : PageModel
{
    private readonly IActivityService _activityService;

    public LogModel(IActivityService activityService)
    {
        _activityService = activityService;
    }

    [BindProperty]
    public ActivityInputModel Input { get; set; } = new();

    public IReadOnlyList<ActivityOfficialOption> Officials { get; set; } = Array.Empty<ActivityOfficialOption>();
    public IReadOnlyList<ActivityView> Activities { get; set; } = Array.Empty<ActivityView>();

    [TempData]
    public string? StatusMessage { get; set; }

    public string? ErrorMessage { get; set; }

    public string CurrentOfficialId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    public async Task OnGetAsync()
    {
        Input.ParticipantIds = new List<string> { CurrentOfficialId };
        await LoadAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadAsync();
            return Page();
        }

        try
        {
            var id = await _activityService.CreateActivityAsync(Input, CurrentOfficialId);
            var names = Input.ParticipantIds.Count;
            StatusMessage = $"Activity {id} saved for {names} official{(names == 1 ? "" : "s")}. The treasury will decide which lines are paid.";
            return RedirectToPage();
        }
        catch (InvalidOperationException ex)
        {
            ErrorMessage = ex.Message;
        }
        catch (SqlException ex)
        {
            ErrorMessage = ex.Message;
        }

        await LoadAsync();
        return Page();
    }

    private async Task LoadAsync()
    {
        Officials = await _activityService.GetOfficialsAsync();
        Activities = await _activityService.GetMyActivitiesAsync(CurrentOfficialId);
    }
}
