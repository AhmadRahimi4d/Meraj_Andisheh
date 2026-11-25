using MerajAndisheh.Core.Abstractions;
using Microsoft.Extensions.Logging;

namespace MerajAndisheh.Infrastructure.Messaging;

/// <summary>
/// نسخه‌ی Dev: ایمیل را نمی‌فرستد و فقط در لاگ می‌نویسد (لینک تأیید و بازنشانی را از لاگ بردار).
/// در مرحله‌ی ۲۲ با ارسال واقعی (SMTP) جایگزین می‌شود.
/// </summary>
public sealed class LogEmailSender(ILogger<LogEmailSender> logger) : IEmailSender
{
    public Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("DEV EMAIL | To: {To} | Subject: {Subject} | Body: {Body}", toEmail, subject, htmlBody);
        return Task.CompletedTask;
    }
}
