using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UASU_VoucherApprovals.Services;

namespace UASU_VoucherApprovals.Pages.Activities;

// [Authorize] only, same as payment acknowledgement attachments - evidence
// is viewable by any signed-in user who has the link.
[Authorize]
public class EvidenceModel : PageModel
{
    private readonly IActivityService _activityService;

    public EvidenceModel(IActivityService activityService)
    {
        _activityService = activityService;
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var file = await _activityService.GetEvidenceAsync(id);
        if (file is null)
            return NotFound();

        return File(file.File_Data, file.Content_Type, file.File_Name);
    }
}
