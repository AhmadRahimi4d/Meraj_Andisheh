namespace MerajAndisheh.Core.Services;

/// <summary>تعریف یک تنظیم سایت: کلید، نوع و حدود مجاز.</summary>
public sealed record SiteSettingDefinition(string Key, SiteSettingKind Kind, int Min, int Max);
