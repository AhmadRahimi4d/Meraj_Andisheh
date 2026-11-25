namespace MerajAndisheh.Core.Abstractions;

/// <summary>
/// ارسال ایمیل. در توسعه، نسخه‌ی Dev متن را در لاگ می‌نویسد؛ SMTP واقعی در مرحله‌ی ۲۲ می‌آید.
/// </summary>
public interface IEmailSender
{
    Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken = default);
}
