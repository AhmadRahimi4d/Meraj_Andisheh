namespace MerajAndisheh.Core.Services;

/// <summary>نتیجه‌ی اعتبارسنجی یک مقدار تنظیم: یا مقدار یکسان‌سازی‌شده، یا خطا.</summary>
public sealed record SiteSettingValidation(string? Normalized, SiteSettingError? Error)
{
    public bool IsValid => Error is null;
}
