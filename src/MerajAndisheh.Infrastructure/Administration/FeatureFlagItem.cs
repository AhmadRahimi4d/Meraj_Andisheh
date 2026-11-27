namespace MerajAndisheh.Infrastructure.Administration;

/// <summary>یک ماژول اختیاری و وضعیت روشن/خاموش آن.</summary>
public sealed record FeatureFlagItem(string Key, bool IsEnabled);
