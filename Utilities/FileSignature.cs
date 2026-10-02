namespace UASU_VoucherApprovals.Utilities;

// Works out a file's type from its first bytes, not its extension or the
// browser-supplied Content-Type, both of which the uploader controls.
public static class FileSignature
{
    public static bool TryDetectImageOrPdf(byte[] data, out string? contentType)
    {
        if (data.Length >= 3 && data[0] == 0xFF && data[1] == 0xD8 && data[2] == 0xFF)
        {
            contentType = "image/jpeg";
            return true;
        }

        if (data.Length >= 8 &&
            data[0] == 0x89 && data[1] == 0x50 && data[2] == 0x4E && data[3] == 0x47 &&
            data[4] == 0x0D && data[5] == 0x0A && data[6] == 0x1A && data[7] == 0x0A)
        {
            contentType = "image/png";
            return true;
        }

        // "%PDF-"
        if (data.Length >= 5 &&
            data[0] == 0x25 && data[1] == 0x50 && data[2] == 0x44 && data[3] == 0x46 && data[4] == 0x2D)
        {
            contentType = "application/pdf";
            return true;
        }

        contentType = null;
        return false;
    }
}
