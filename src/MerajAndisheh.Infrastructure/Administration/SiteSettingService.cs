using MerajAndisheh.Core.Domain.Entities;
using MerajAndisheh.Core.Services;
using MerajAndisheh.Infrastructure.Data;
using MerajAndisheh.Infrastructure.Data.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MerajAndisheh.Infrastructure.Administration;

/// <summary>
/// خواندن و ذخیره‌ی تنظیمات سایت (SiteSetting) با اعتبارسنجی SiteSettingRules.
/// ذخیره «همه یا هیچ» است و هر تغییر در لاگ ثبت می‌شود (چه کسی، چه کلیدی، از چه مقدار به چه مقدار).
/// </summary>
public sealed class SiteSettingService(AppDbContext db, ILogger<SiteSettingService> logger)
{
    /// <summary>همه‌ی تنظیمات قابل ویرایش به ترتیب SiteSettingRules.All؛ اگر ردیفی نبود، مقدار پیش‌فرض Seed.</summary>
    public async Task<IReadOnlyList<SiteSettingItem>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var stored = await db.SiteSettings.ToDictionaryAsync(s => s.Key, s => s.Value, cancellationToken);

        return SiteSettingRules.All.Select(definition =>
        {
            var value = stored.TryGetValue(definition.Key, out var existing)
                ? existing
                : SeedData.SiteSettings.GetValueOrDefault(definition.Key, string.Empty);
            return new SiteSettingItem(definition, value);
        }).ToList();
    }

    public async Task<SiteSettingsUpdateResult> UpdateAsync(IReadOnlyDictionary<string, string?> values, int actingUserId, CancellationToken cancellationToken = default)
    {
        var errors = new Dictionary<string, SiteSettingError>(StringComparer.Ordinal);
        var normalized = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (var (key, raw) in values)
        {
            var validation = SiteSettingRules.Validate(key, raw);
            if (validation.IsValid) normalized[key] = validation.Normalized!;
            else errors[key] = validation.Error!.Value;
        }

        if (errors.Count > 0) return new SiteSettingsUpdateResult(errors); // هیچ‌چیز ذخیره نمی‌شود

        var keys = normalized.Keys.ToList();
        var rows = await db.SiteSettings
            .Where(s => keys.Contains(s.Key))
            .ToDictionaryAsync(s => s.Key, cancellationToken);

        var changes = new List<(string Key, string? Old, string New)>();
        foreach (var (key, value) in normalized)
        {
            if (rows.TryGetValue(key, out var row))
            {
                if (row.Value == value) continue;
                changes.Add((key, row.Value, value));
                row.Value = value;
            }
            else
            {
                changes.Add((key, null, value));
                db.SiteSettings.Add(new SiteSetting { Key = key, Value = value });
            }
        }

        if (changes.Count == 0) return new SiteSettingsUpdateResult(errors);

        await db.SaveChangesAsync(cancellationToken);

        foreach (var (key, oldValue, newValue) in changes)
        {
            logger.LogInformation("Site setting {Key} changed by user {ActorId}: {OldValue} -> {NewValue}",
                key, actingUserId, oldValue, newValue);
        }

        return new SiteSettingsUpdateResult(errors);
    }
}
