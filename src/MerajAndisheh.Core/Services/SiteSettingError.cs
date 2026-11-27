namespace MerajAndisheh.Core.Services;

/// <summary>دلیل نامعتبر بودن یک مقدار تنظیم. فقط کد است؛ متن فارسی از resx می‌آید.</summary>
public enum SiteSettingError
{
    UnknownKey = 1,
    Required,
    TooLong,
    NotInteger,
    OutOfRange,
    NotBool
}
