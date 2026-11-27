using MerajAndisheh.Core.Domain;
using MerajAndisheh.Core.Domain.Entities;
using MerajAndisheh.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MerajAndisheh.Infrastructure.Administration;

/// <summary>
/// روشن/خاموش‌کردن ماژول‌های اختیاری (FeatureKeys). هر تغییر ثبت می‌شود (لاگ + مقدار در جدول FeatureFlags).
/// اجبار ۴۰۴ برای ماژول خاموش و پنهان‌شدن لینک منو در مرحله‌ی خودِ هر ماژول ساخته می‌شود.
/// </summary>
public sealed class FeatureFlagService(AppDbContext db, ILogger<FeatureFlagService> logger)
{
    /// <summary>همه‌ی ماژول‌ها به ترتیب FeatureKeys.All؛ ردیف ناموجود یعنی خاموش.</summary>
    public async Task<IReadOnlyList<FeatureFlagItem>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var stored = await db.FeatureFlags.ToDictionaryAsync(f => f.Key, f => f.IsEnabled, cancellationToken);

        return FeatureKeys.All
            .Select(key => new FeatureFlagItem(key, stored.GetValueOrDefault(key)))
            .ToList();
    }

    public async Task<FeatureToggleResult> SetAsync(string? key, bool enabled, int actingUserId, CancellationToken cancellationToken = default)
    {
        if (key is null || !FeatureKeys.All.Contains(key, StringComparer.Ordinal))
            return new FeatureToggleResult(false, false);

        var flag = await db.FeatureFlags.SingleOrDefaultAsync(f => f.Key == key, cancellationToken);
        var previous = flag?.IsEnabled ?? false;

        if (flag is null)
        {
            db.FeatureFlags.Add(new FeatureFlag { Key = key, IsEnabled = enabled });
        }
        else
        {
            flag.IsEnabled = enabled;
        }

        if (previous == enabled && flag is not null) return new FeatureToggleResult(true, false);

        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Feature {Feature} {State} by user {ActorId}", key, enabled ? "enabled" : "disabled", actingUserId);
        return new FeatureToggleResult(true, previous != enabled);
    }
}
