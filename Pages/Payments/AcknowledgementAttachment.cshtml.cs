using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UASU_VoucherApprovals.Services;

namespace UASU_VoucherApprovals.Pages.Payments;

// [Authorize] only, no role restriction - viewing acknowledgement
// evidence isn't role-gated any more tightly than Reports is.
[Authorize]
public class AcknowledgementAttachmentModel : PageModel
{
    private readonly IVoucherService _voucherService;

    public AcknowledgementAttachmentModel(IVoucherService voucherService)
    {
        _voucherService = voucherService;
    }

    public async Task<IActionResult> OnGetAsync(string paymentId)
    {
        var attachment = await _voucherService.GetAcknowledgementAttachmentAsync(paymentId);
        if (attachment is null)
            return NotFound();

        return File(attachment.Attachment_Data, attachment.Attachment_ContentType, attachment.Attachment_FileName);
    }
}
