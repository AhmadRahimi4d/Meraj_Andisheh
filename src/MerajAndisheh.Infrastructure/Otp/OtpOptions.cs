namespace MerajAndisheh.Infrastructure.Otp;

/// <summary>
/// تنظیمات OTP از بخش «Otp» (User Secrets در توسعه، تنظیمات سرور در تولید).
/// کلید محرمانه‌ی هش کدها هرگز در کد یا مخزن نیست؛ اسکریپت مرحله‌ی ۵ در توسعه آن را می‌سازد.
/// </summary>
public sealed class OtpOptions
{
    public const string SectionName = "Otp";

    /// <summary>حداقل طول کلید هش.</summary>
    public const int MinKeyLength = 32;

    /// <summary>کلید HMAC برای هش‌کردن کدها (Otp:HashKey).</summary>
    public string? HashKey { get; set; }
}
