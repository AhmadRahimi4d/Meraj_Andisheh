namespace MerajAndisheh.Core.Domain.Entities;

/// <summary>کلید روشن/خاموش یک ماژول اختیاری (کلیدها در FeatureKeys).</summary>
public class FeatureFlag
{
    public string Key { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
}
