namespace MerajAndisheh.Core.Domain;

/// <summary>قواعد ثابت کد یک‌بارمصرف پیامکی (OTP) — بخش ۳.۱۴ سند.</summary>
public static class OtpRules
{
    /// <summary>تعداد رقم‌های کد.</summary>
    public const int CodeLength = 6;

    /// <summary>مدت اعتبار هر کد (دقیقه).</summary>
    public const int ValidityMinutes = 2;

    /// <summary>حداکثر تلاش غلط برای هر کد؛ تلاش پنجم کد را قفل می‌کند.</summary>
    public const int MaxAttempts = 5;

    /// <summary>حداقل فاصله‌ی دو ارسال برای یک شماره (ثانیه).</summary>
    public const int ResendCooldownSeconds = 60;

    /// <summary>حداکثر تعداد کد در هر ساعت برای هر شماره.</summary>
    public const int MaxCodesPerHour = 5;
}
