using MerajAndisheh.Core.Domain;
using MerajAndisheh.Core.Services;
using MerajAndisheh.Infrastructure.Administration;
using MerajAndisheh.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MerajAndisheh.Tests;

/// <summary>تنظیمات و ماژول‌ها (settings.manage): ذخیره‌ی اعتبارسنجی‌شده، همه‌یا‌هیچ، و ثبت تغییر در لاگ.</summary>
public class SiteConfigurationServiceTests
{
    private const int Actor = 7;

    private static Dictionary<string, string?> Values(params (string Key, string? Value)[] pairs)
        => pairs.ToDictionary(p => p.Key, p => p.Value, StringComparer.Ordinal);

    // ---------- تنظیمات سایت ----------

    [Fact]
    public async Task GetAll_ReturnsEverySetting_InRulesOrder_WithSeededValues()
    {
        using var host = await AdminTestHost.CreateAsync();
        using var scope = host.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<SiteSettingService>();

        var items = await service.GetAllAsync();

        Assert.Equal(SiteSettingRules.All.Select(d => d.Key), items.Select(i => i.Definition.Key));
        Assert.Equal("3", items.Single(i => i.Definition.Key == SiteSettingKeys.PersonalizationMinViews).Value);
        Assert.Equal("1000", items.Single(i => i.Definition.Key == SiteSettingKeys.CommentsMaxLength).Value);
    }

    [Fact]
    public async Task Update_ValidValues_ArePersisted_WithLatinDigits()
    {
        using var host = await AdminTestHost.CreateAsync();
        using var scope = host.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<SiteSettingService>();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var result = await service.UpdateAsync(Values(
            (SiteSettingKeys.SiteTitle, "  عنوان تازه  "),
            (SiteSettingKeys.PersonalizationMinViews, "۵")), Actor);

        Assert.True(result.Succeeded);
        Assert.Equal("عنوان تازه", (await db.SiteSettings.SingleAsync(s => s.Key == SiteSettingKeys.SiteTitle)).Value);
        Assert.Equal("5", (await db.SiteSettings.SingleAsync(s => s.Key == SiteSettingKeys.PersonalizationMinViews)).Value);
    }

    [Fact]
    public async Task Update_OneInvalidValue_SavesNothing_AndReportsEachError()
    {
        using var host = await AdminTestHost.CreateAsync();
        using var scope = host.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<SiteSettingService>();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var originalTitle = (await db.SiteSettings.SingleAsync(s => s.Key == SiteSettingKeys.SiteTitle)).Value;

        var result = await service.UpdateAsync(Values(
            (SiteSettingKeys.SiteTitle, "عنوان تازه"),
            (SiteSettingKeys.PersonalizationMinViews, "0"),
            ("Not.A.Setting", "x")), Actor);

        Assert.False(result.Succeeded);
        Assert.Equal(SiteSettingError.OutOfRange, result.Errors[SiteSettingKeys.PersonalizationMinViews]);
        Assert.Equal(SiteSettingError.UnknownKey, result.Errors["Not.A.Setting"]);
        Assert.False(result.Errors.ContainsKey(SiteSettingKeys.SiteTitle));

        db.ChangeTracker.Clear();
        Assert.Equal(originalTitle, (await db.SiteSettings.SingleAsync(s => s.Key == SiteSettingKeys.SiteTitle)).Value);
        Assert.Equal("3", (await db.SiteSettings.SingleAsync(s => s.Key == SiteSettingKeys.PersonalizationMinViews)).Value);
    }

    [Fact]
    public async Task Update_LogsEveryChange_WithKeyUserAndValues()
    {
        using var host = await AdminTestHost.CreateAsync();
        using var scope = host.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<SiteSettingService>();

        await service.UpdateAsync(Values((SiteSettingKeys.PersonalizationMinViews, "9")), Actor);

        Assert.Contains(host.Logs.Messages, m =>
            m.Contains("Site setting") && m.Contains(SiteSettingKeys.PersonalizationMinViews)
            && m.Contains("user 7") && m.Contains("3 -> 9"));
    }

    [Fact]
    public async Task Update_SameValues_SucceedsWithoutLogging()
    {
        using var host = await AdminTestHost.CreateAsync();
        using var scope = host.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<SiteSettingService>();

        var result = await service.UpdateAsync(Values((SiteSettingKeys.PersonalizationMinViews, "3")), Actor);

        Assert.True(result.Succeeded);
        Assert.DoesNotContain(host.Logs.Messages, m => m.Contains("Site setting"));
    }

    // ---------- ماژول‌ها ----------

    [Fact]
    public async Task Features_StartOff_AndInFeatureKeysOrder()
    {
        using var host = await AdminTestHost.CreateAsync();
        using var scope = host.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<FeatureFlagService>();

        var items = await service.GetAllAsync();

        Assert.Equal(FeatureKeys.All, items.Select(i => i.Key));
        Assert.All(items, item => Assert.False(item.IsEnabled));
    }

    [Fact]
    public async Task Feature_TurnOnAndOff_IsPersisted_AndLogged()
    {
        using var host = await AdminTestHost.CreateAsync();
        using var scope = host.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<FeatureFlagService>();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var on = await service.SetAsync(FeatureKeys.Comments, true, Actor);

        Assert.True(on.Succeeded);
        Assert.True(on.Changed);
        Assert.True((await db.FeatureFlags.SingleAsync(f => f.Key == FeatureKeys.Comments)).IsEnabled);
        Assert.Contains(host.Logs.Messages, m => m.Contains("Feature Comments enabled by user 7"));

        var off = await service.SetAsync(FeatureKeys.Comments, false, Actor);

        Assert.True(off.Changed);
        Assert.False((await db.FeatureFlags.SingleAsync(f => f.Key == FeatureKeys.Comments)).IsEnabled);
        Assert.Contains(host.Logs.Messages, m => m.Contains("Feature Comments disabled by user 7"));
    }

    [Fact]
    public async Task Feature_SettingTheSameState_IsNotAChange_AndNotLogged()
    {
        using var host = await AdminTestHost.CreateAsync();
        using var scope = host.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<FeatureFlagService>();
        await service.SetAsync(FeatureKeys.Films, true, Actor);

        // فقط پیام‌های خود سرویس ماژول شمرده می‌شوند (لاگ‌های EF Core مثل «Executed DbCommand» هم در همین ارائه‌دهنده می‌آیند)
        int FeatureLogCount() => host.Logs.Messages.Count(m => m.Contains("Feature Films"));
        var before = FeatureLogCount();
        Assert.Equal(1, before);

        var again = await service.SetAsync(FeatureKeys.Films, true, Actor);

        Assert.True(again.Succeeded);
        Assert.False(again.Changed);
        Assert.Equal(before, FeatureLogCount());
    }

    [Fact]
    public async Task Feature_UnknownKey_IsRejected()
    {
        using var host = await AdminTestHost.CreateAsync();
        using var scope = host.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<FeatureFlagService>();

        Assert.False((await service.SetAsync("Teleport", true, Actor)).Succeeded);
        Assert.False((await service.SetAsync(null, true, Actor)).Succeeded);
    }

    [Fact]
    public async Task Feature_ChangeIsVisibleInTheList()
    {
        using var host = await AdminTestHost.CreateAsync();
        using var scope = host.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<FeatureFlagService>();

        await service.SetAsync(FeatureKeys.Books, true, Actor);
        var items = await service.GetAllAsync();

        Assert.True(items.Single(i => i.Key == FeatureKeys.Books).IsEnabled);
        Assert.False(items.Single(i => i.Key == FeatureKeys.Games).IsEnabled);
    }
}
