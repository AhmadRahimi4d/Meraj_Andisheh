namespace MerajAndisheh.Core.Services;

/// <summary>نوع مقدار یک تنظیم سایت (برای ساخت فرم و اعتبارسنجی).</summary>
public enum SiteSettingKind
{
    /// <summary>متن؛ Min و Max طول متن است.</summary>
    Text = 1,

    /// <summary>روشن/خاموش با مقدار «1» یا «0».</summary>
    Bool,

    /// <summary>عدد صحیح؛ Min و Max بازه‌ی مجاز است.</summary>
    Int
}
