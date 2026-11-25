namespace MerajAndisheh.Core.Domain;

/// <summary>قواعد ثابت حساب کاربری (بخش ۳.۱۴ سند).</summary>
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
}
