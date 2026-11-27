using MerajAndisheh.Core.Services;

namespace MerajAndisheh.Infrastructure.Administration;

/// <summary>نتیجه‌ی ذخیره‌ی تنظیمات: یا همه ذخیره شد، یا هیچ‌کدام و خطای هر کلید در Errors است.</summary>
public sealed record SiteSettingsUpdateResult(IReadOnlyDictionary<string, SiteSettingError> Errors)
{
    public bool Succeeded => Errors.Count == 0;
}
