namespace MerajAndisheh.Infrastructure.Administration;

/// <summary>Succeeded = کلید شناخته‌شده بود؛ Changed = وضعیت واقعاً عوض شد (تکرار همان وضعیت، تغییر حساب نمی‌شود).</summary>
public sealed record FeatureToggleResult(bool Succeeded, bool Changed);
