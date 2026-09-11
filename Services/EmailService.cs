using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace UASU_VoucherApprovals.Services;

public interface IEmailService
{
    // Returns true if a real send was attempted via SMTP, false if it
    // fell back to logging only (no EmailSettings:Host configured) -
    // callers use this to decide whether an on-screen dev preview of
    // the code is still needed, rather than assuming "Development
    // environment" implies "nothing was actually emailed".
    Task<bool> SendVerificationCodeAsync(string toEmail, string recipientName, string code);
}

// Built on MailKit rather than the framework's own System.Net.Mail.
// SmtpClient - that legacy client (Microsoft's own docs mark it
// obsolete) negotiates STARTTLS-then-AUTH loosely enough that some
// providers reject it outright (Brevo included: "550 5.7.0 Please
// authenticate first", meaning the AUTH step silently never completed).
// MailKit's explicit Connect/Authenticate/Send sequence is what
// Microsoft itself recommends replacing SmtpClient with. Configured via
// the EmailSettings section (Host/Port/Username/Password/FromAddress),
// same pattern as the UasuFinance connection string: real values come
// from user-secrets in Development or EmailSettings__* environment
// variables in production, never committed to appsettings.json.
public class SmtpEmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(IConfiguration configuration, ILogger<SmtpEmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<bool> SendVerificationCodeAsync(string toEmail, string recipientName, string code)
    {
        var host = _configuration["EmailSettings:Host"];
        if (string.IsNullOrWhiteSpace(host))
        {
            // No SMTP server configured yet - log instead of throwing so
            // the verification flow can still be exercised end-to-end
            // (Pages/Account/VerifyEmail shows the code directly in this
            // case, but only when running in Development).
            _logger.LogWarning("EmailSettings:Host not configured - verification code for {Email} would be: {Code}", toEmail, code);
            return false;
        }

        var port = _configuration.GetValue("EmailSettings:Port", 587);
        var enableSsl = _configuration.GetValue("EmailSettings:EnableSsl", true);
        var username = _configuration["EmailSettings:Username"];
        var password = _configuration["EmailSettings:Password"];
        var fromAddress = _configuration["EmailSettings:FromAddress"] ?? username ?? "no-reply@uasu.example";
        var fromName = _configuration["EmailSettings:FromName"] ?? "MMU UASU Finance App";

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(fromName, fromAddress));
        message.To.Add(new MailboxAddress(recipientName, toEmail));
        message.Subject = "Verify your MMU UASU Finance App login";
        message.Body = new TextPart("plain")
        {
            Text = $"Hello {recipientName},\n\nYour verification code is: {code}\n\n" +
                   "This code expires in 15 minutes. If you didn't request this, contact your Treasurer."
        };

        using var client = new SmtpClient();

        // StartTls when EnableSsl is true (port 587 - upgrade after
        // connecting in plain text), SslOnConnect for implicit-TLS ports
        // like 465. Auto lets MailKit pick correctly either way if this
        // ever runs against a provider on a different port.
        var socketOptions = enableSsl
            ? (port == 465 ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls)
            : SecureSocketOptions.None;

        await client.ConnectAsync(host, port, socketOptions);

        if (!string.IsNullOrEmpty(username))
            await client.AuthenticateAsync(username, password ?? string.Empty);

        await client.SendAsync(message);
        await client.DisconnectAsync(true);

        return true;
    }
}
