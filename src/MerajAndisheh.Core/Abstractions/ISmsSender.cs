namespace MerajAndisheh.Core.Abstractions;

/// <summary>
/// ارسال پیامک. فعلاً فقط «کد تأیید»؛ چون بیشتر ارائه‌دهنده‌های ایرانی برای کد تأیید قالب (Template) دارند،
/// خود ارائه‌دهنده متن را می‌سازد. نسخه‌ی Dev کد را در لاگ می‌نویسد؛ ارائه‌دهنده‌ی واقعی پیش از مرحله‌ی ۲۲ انتخاب می‌شود.
/// </summary>
public interface ISmsSender
{
    Task SendOtpCodeAsync(string phoneNumber, string code, CancellationToken cancellationToken = default);
}
