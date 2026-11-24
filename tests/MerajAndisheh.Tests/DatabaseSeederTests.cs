using MerajAndisheh.Core.Domain;
using MerajAndisheh.Core.Domain.Entities;
using MerajAndisheh.Core.Domain.Enums;
using MerajAndisheh.Core.Services;
using MerajAndisheh.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MerajAndisheh.Tests;

/// <summary>معیارهای مرحله‌ی ۳: داده‌ی اولیه و تکرارپذیر بودن آن.</summary>
public class DatabaseSeederTests
{
    [Fact]
    public async Task Seed_CreatesExpectedInitialData()
    {
        using var database = new SqliteTestDatabase();
        await using var db = database.CreateContext();

        await DatabaseSeeder.SeedAsync(db);

        Assert.Equal(4, await db.AgeGroups.CountAsync());
        Assert.Equal(3, await db.ImpactLevels.CountAsync());
        Assert.Equal(10, await db.Topics.CountAsync());
        Assert.Equal(FeatureKeys.All.Count, await db.FeatureFlags.CountAsync());
        Assert.Equal(8, await db.SiteSettings.CountAsync());
    }

    [Fact]
    public async Task Seed_RunTwice_CreatesNoDuplicates()
    {
        using var database = new SqliteTestDatabase();
        await using var db = database.CreateContext();

        await DatabaseSeeder.SeedAsync(db);
        var before = await CountAllAsync(db);

        await DatabaseSeeder.SeedAsync(db);
        var after = await CountAllAsync(db);

        Assert.Equal(before, after);
    }

    [Fact]
    public async Task Seed_AllOptionalModules_AreOff()
    {
        using var database = new SqliteTestDatabase();
        await using var db = database.CreateContext();

        await DatabaseSeeder.SeedAsync(db);

        Assert.All(await db.FeatureFlags.ToListAsync(), flag => Assert.False(flag.IsEnabled));
    }

    [Fact]
    public async Task Seed_AgeGroups_MatchDesignDoc()
    {
        using var database = new SqliteTestDatabase();
        await using var db = database.CreateContext();

        await DatabaseSeeder.SeedAsync(db);

        var child = await db.AgeGroups.SingleAsync(a => a.Key == "child");
        Assert.Equal(7, child.MinAge);
        Assert.Equal(11, child.MaxAge!.Value);
        Assert.Equal(ThemeCatalog.Playful, child.ThemeKey);

        var adult = await db.AgeGroups.SingleAsync(a => a.Key == "adult");
        Assert.Equal(21, adult.MinAge);
        Assert.Null(adult.MaxAge);
        Assert.Equal(ThemeCatalog.Editorial, adult.ThemeKey);
    }

    [Fact]
    public async Task Seed_Topics_FormTwoLevelTree_WithUniqueSlugs()
    {
        using var database = new SqliteTestDatabase();
        await using var db = database.CreateContext();

        await DatabaseSeeder.SeedAsync(db);

        var topics = await db.Topics.ToListAsync();
        var roots = topics.Where(t => t.ParentId == null).ToList();
        var children = topics.Where(t => t.ParentId != null).ToList();

        Assert.Equal(5, roots.Count);
        Assert.Equal(5, children.Count);
        Assert.All(children, c => Assert.Contains(roots, r => r.Id == c.ParentId));
        Assert.Equal(topics.Count, topics.Select(t => t.Slug).Distinct().Count());
    }

    [Fact]
    public async Task Seed_DoesNotOverwriteAdminChanges()
    {
        using var database = new SqliteTestDatabase();
        await using var db = database.CreateContext();
        await DatabaseSeeder.SeedAsync(db);

        var child = await db.AgeGroups.SingleAsync(a => a.Key == "child");
        child.MinAge = 6;
        var films = await db.FeatureFlags.SingleAsync(f => f.Key == FeatureKeys.Films);
        films.IsEnabled = true;
        var title = await db.SiteSettings.SingleAsync(s => s.Key == SiteSettingKeys.SiteTitle);
        title.Value = "عنوان دلخواه";
        await db.SaveChangesAsync();

        await DatabaseSeeder.SeedAsync(db);

        Assert.Equal(6, (await db.AgeGroups.SingleAsync(a => a.Key == "child")).MinAge);
        Assert.True((await db.FeatureFlags.SingleAsync(f => f.Key == FeatureKeys.Films)).IsEnabled);
        Assert.Equal("عنوان دلخواه", (await db.SiteSettings.SingleAsync(s => s.Key == SiteSettingKeys.SiteTitle)).Value);
    }

    [Fact]
    public async Task ContentItem_WithDuplicateSlug_IsRejectedByDatabase()
    {
        using var database = new SqliteTestDatabase();
        await using var db = database.CreateContext();

        db.ContentItems.Add(NewContent("مسیر-اقتصاد"));
        await db.SaveChangesAsync();

        db.ContentItems.Add(NewContent("مسیر-اقتصاد"));

        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }

    private static ContentItem NewContent(string slug) => new()
    {
        Title = "عنوان",
        Slug = slug,
        Summary = "خلاصه",
        Format = ContentFormat.Article,
        CreatedById = 1,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };

    private static async Task<int[]> CountAllAsync(AppDbContext db) =>
    [
        await db.AgeGroups.CountAsync(),
        await db.ImpactLevels.CountAsync(),
        await db.Topics.CountAsync(),
        await db.FeatureFlags.CountAsync(),
        await db.SiteSettings.CountAsync()
    ];
}
