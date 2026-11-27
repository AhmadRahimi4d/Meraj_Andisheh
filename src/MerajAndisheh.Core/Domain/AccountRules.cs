namespace MerajAndisheh.Core.Domain;

/// <summary>قواعد ثابت حساب کاربری (بخش ۳.۱۴ و ۴ سند).</summary>
public static class AccountRules
{
    /// <summary>حداقل طول رمز عبور (فقط طول؛ سند قاعده‌ی دیگری نگذاشته).</summary>
    public const int MinPasswordLength = 8;

    /// <summary>تعداد ورود ناموفق پیش از قفل.</summary>
    public const int MaxFailedAttempts = 5;

    /// <summary>مدت قفل حساب (دقیقه).</summary>
    public const int LockoutMinutes = 15;

    /// <summary>سقف طول نام نمایشی (ستون nvarchar(60)).</summary>
    public const int DisplayNameMaxLength = 60;

    /// <summary>مدت اعتبار نشست عضو (روز).</summary>
    public const int MemberSessionDays = 30;

    /// <summary>مدت اعتبار نشست همکار و مدیر کل (ساعت)؛ بدون تمدید خودکار.</summary>
    public const int StaffSessionHours = 8;

    /// <summary>
    /// هر چند دقیقه یک‌بار اعتبار کوکی با دیتابیس سنجیده می‌شود. غیرفعال‌شدن حساب یا تغییر دسترسی
    /// (که Security Stamp را عوض می‌کند) حداکثر تا همین مدت بعد، نشست‌های باز را از کار می‌اندازد.
    /// </summary>
    public const int SessionValidationMinutes = 1;
}
