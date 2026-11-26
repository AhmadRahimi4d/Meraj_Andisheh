using System.Security.Cryptography;
using Microsoft.AspNetCore.DataProtection;

namespace MerajAndisheh.Web.Identity;

/// <summary>
/// «برگه‌ی تأیید شماره»: پس از درست‌بودن کد پیامکی برای شماره‌ی بدون حساب، این برگه (رمزنگاری‌شده و ۱۰ دقیقه‌ای)
/// به صفحه‌ی تکمیل پروفایل می‌رود تا بی‌نیاز از ذخیره‌ی حساب نیمه‌کاره ثابت شود شماره تأیید شده است.
/// با Data Protection امضا می‌شود، پس دستکاری‌شدنی و جعل‌شدنی نیست.
/// </summary>
public sealed class OtpVerificationTicket(IDataProtectionProvider provider)
{
    private static readonly TimeSpan Lifetime = TimeSpan.FromMinutes(10);

    private readonly ITimeLimitedDataProtector _protector =
        provider.CreateProtector("MerajAndisheh.OtpVerifiedPhone.v1").ToTimeLimitedDataProtector();

    public string Create(string phoneNumber) => _protector.Protect(phoneNumber, Lifetime);

    /// <summary>شماره‌ی تأییدشده، یا null اگر برگه نامعتبر یا منقضی باشد.</summary>
    public string? Validate(string? ticket)
    {
        if (string.IsNullOrWhiteSpace(ticket)) return null;

        try
        {
            return _protector.Unprotect(ticket);
        }
        catch (CryptographicException)
        {
            return null;
        }
    }
}
