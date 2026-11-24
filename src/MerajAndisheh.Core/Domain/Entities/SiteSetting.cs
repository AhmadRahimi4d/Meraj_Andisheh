namespace MerajAndisheh.Core.Domain.Entities;

/// <summary>تنظیم سایت به‌صورت کلید و مقدار (کلیدها در SiteSettingKeys).</summary>
public class SiteSetting
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}
