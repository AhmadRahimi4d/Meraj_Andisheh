using MerajAndisheh.Core.Services;

namespace MerajAndisheh.Infrastructure.Administration;

/// <summary>یک تنظیم سایت با مقدار فعلی (برای ساخت فرم).</summary>
public sealed record SiteSettingItem(SiteSettingDefinition Definition, string Value);
