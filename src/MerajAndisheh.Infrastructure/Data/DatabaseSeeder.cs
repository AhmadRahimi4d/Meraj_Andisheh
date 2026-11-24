using MerajAndisheh.Core.Domain;
using MerajAndisheh.Core.Domain.Entities;
using MerajAndisheh.Core.Services;
using MerajAndisheh.Infrastructure.Data.Seed;
using Microsoft.EntityFrameworkCore;

namespace MerajAndisheh.Infrastructure.Data;

/// <summary>
/// داده‌ی اولیه. تکرارپذیر است: اجرای چندباره چیزی تکراری نمی‌سازد و ویرایش‌های مدیر را رونویسی نمی‌کند.
/// - رده‌ی سنی، کلید ماژول، تنظیمات: بر اساس کلید، فقط موارد ناموجود اضافه می‌شوند.
/// - سطح اثرگذاری و موضوعات: فقط وقتی جدول کاملاً خالی است (اولین اجرا).
/// </summary>
public static class DatabaseSeeder
{
    public static async Task SeedAsync(AppDbContext db, CancellationToken cancellationToken = default)
    {
        await SeedAgeGroupsAsync(db, cancellationToken);
        await SeedImpactLevelsAsync(db, cancellationToken);
        await SeedTopicsAsync(db, cancellationToken);
        await SeedFeatureFlagsAsync(db, cancellationToken);
        await SeedSiteSettingsAsync(db, cancellationToken);
    }

    private static async Task SeedAgeGroupsAsync(AppDbContext db, CancellationToken ct)
    {
        var existing = await db.AgeGroups.Select(a => a.Key).ToListAsync(ct);

        foreach (var seed in SeedData.AgeGroups.Where(s => !existing.Contains(s.Key)))
        {
            db.AgeGroups.Add(new AgeGroup
            {
                Key = seed.Key,
                Name = seed.Name,
                MinAge = seed.MinAge,
                MaxAge = seed.MaxAge,
                ThemeKey = seed.ThemeKey,
                SortOrder = seed.SortOrder,
                IsActive = true
            });
        }

        await db.SaveChangesAsync(ct);
    }

    private static async Task SeedImpactLevelsAsync(AppDbContext db, CancellationToken ct)
    {
        if (await db.ImpactLevels.AnyAsync(ct)) return;

        var order = 1;
        foreach (var name in SeedData.ImpactLevels)
        {
            db.ImpactLevels.Add(new ImpactLevel { Name = name, SortOrder = order++ });
        }

        await db.SaveChangesAsync(ct);
    }

    private static async Task SeedTopicsAsync(AppDbContext db, CancellationToken ct)
    {
        if (await db.Topics.AnyAsync(ct)) return;

        var usedSlugs = new List<string>();
        var rootOrder = 1;

        foreach (var rootSeed in SeedData.Topics)
        {
            var root = new Topic
            {
                Name = rootSeed.Name,
                Slug = NextSlug(rootSeed.Name, usedSlugs),
                SortOrder = rootOrder++,
                IsActive = true
            };

            var childOrder = 1;
            foreach (var childName in rootSeed.Children)
            {
                root.Children.Add(new Topic
                {
                    Name = childName,
                    Slug = NextSlug(childName, usedSlugs),
                    SortOrder = childOrder++,
                    IsActive = true
                });
            }

            db.Topics.Add(root); // فرزندان همراه والد (با کلید خارجی درست) ذخیره می‌شوند
        }

        await db.SaveChangesAsync(ct);
    }

    private static async Task SeedFeatureFlagsAsync(AppDbContext db, CancellationToken ct)
    {
        var existing = await db.FeatureFlags.Select(f => f.Key).ToListAsync(ct);

        // همه‌ی ماژول‌های اختیاری در ابتدا خاموش‌اند؛ مرحله‌ی هر ماژول کلید خودش را روشن می‌کند.
        foreach (var key in FeatureKeys.All.Where(k => !existing.Contains(k)))
        {
            db.FeatureFlags.Add(new FeatureFlag { Key = key, IsEnabled = false });
        }

        await db.SaveChangesAsync(ct);
    }

    private static async Task SeedSiteSettingsAsync(AppDbContext db, CancellationToken ct)
    {
        var existing = await db.SiteSettings.Select(s => s.Key).ToListAsync(ct);

        foreach (var (key, value) in SeedData.SiteSettings.Where(kv => !existing.Contains(kv.Key)))
        {
            db.SiteSettings.Add(new SiteSetting { Key = key, Value = value });
        }

        await db.SaveChangesAsync(ct);
    }

    private static string NextSlug(string name, List<string> usedSlugs)
    {
        var slug = SlugGenerator.Generate(name, usedSlugs);
        usedSlugs.Add(slug);
        return slug;
    }
}
