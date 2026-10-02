using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UASU_VoucherApprovals.Services;

namespace UASU_VoucherApprovals.Pages.Floats;

// Same gate as the float pages that link here.
[Authorize(Policy = "TreasuryAdmin")]
public class ReceiptAttachmentModel : PageModel
{
    private readonly ICashFloatService _floatService;

    public ReceiptAttachmentModel(ICashFloatService floatService)
    {
        _floatService = floatService;
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var attachment = await _floatService.GetReceiptAttachmentAsync(id);
        if (attachment is null)
            return NotFound();

        return File(attachment.Attachment_Data, attachment.Attachment_ContentType, attachment.Attachment_FileName);
    }
}
