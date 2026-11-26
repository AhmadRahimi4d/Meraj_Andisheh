using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;

namespace MerajAndisheh.Infrastructure.Otp;

/// <summary>
/// هش کد تأیید با HMAC-SHA256 و کلید محرمانه‌ی سرور. چون کد فقط ۶ رقم است، هش ساده (بدون کلید)
/// با لو رفتن دیتابیس در چند ثانیه شکسته می‌شد. شماره هم داخل پیام هش است تا کد یکسانِ دو شماره، هش یکسان نداشته باشد.
/// کلید نبودن فقط هنگام استفاده خطا می‌دهد، نه هنگام شروع برنامه (تا صفحه‌های بی‌ربط به OTP از کار نیفتند).
/// </summary>
public sealed class OtpCodeHasher(IOptions<OtpOptions> options)
{
    public string Hash(string phoneNumber, string code)
    {
        var key = options.Value.HashKey;
        if (string.IsNullOrWhiteSpace(key) || key.Length < OtpOptions.MinKeyLength)
        {
            throw new InvalidOperationException(
                $"{OtpOptions.SectionName}:HashKey is not configured (at least {OtpOptions.MinKeyLength} characters). " +
                "Set it in User Secrets (development) or server settings (production).");
        }

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
        var bytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(phoneNumber + ":" + code));
        return Convert.ToHexString(bytes);
    }

    /// <summary>مقایسه‌ی زمان‌ثابت (مقاوم در برابر حمله‌ی زمان‌سنجی).</summary>
    public bool Verify(string phoneNumber, string code, string expectedHash)
    {
        var actual = Encoding.ASCII.GetBytes(Hash(phoneNumber, code));
        var expected = Encoding.ASCII.GetBytes(expectedHash ?? string.Empty);
        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }
}
