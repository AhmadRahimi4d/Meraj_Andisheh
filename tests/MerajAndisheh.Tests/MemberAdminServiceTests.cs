using MerajAndisheh.Core.Domain;
using MerajAndisheh.Infrastructure.Administration;
using MerajAndisheh.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MerajAndisheh.Tests;

/// <summary>صفحه‌ی کاربران (users.manage): جستجوی اعضا و فعال/غیرفعال‌سازی.</summary>
public class MemberAdminServiceTests
{
    private const int Actor = 1;

    private static async Task<AdminTestHost> HostWithPeopleAsync()
    {
        var host = await AdminTestHost.CreateAsync();
        await host.CreateUserAsync("ali@example.com", RoleNames.Member, "علی", email: "ali@example.com", phone: "09121234567");
        await host.CreateUserAsync("09351234567", RoleNames.Member, "سارا", phone: "09351234567");
        await host.CreateUserAsync("staff@example.com", RoleNames.Staff, "همکار", email: "staff@example.com");
        await host.CreateUserAsync("boss@example.com", RoleNames.SuperAdmin, "مدیر", email: "boss@example.com");
        return host;
    }

    [Fact]
    public async Task Search_WithoutQuery_ListsOnlyMembers()
    {
        using var host = await HostWithPeopleAsync();
        using var scope = host.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<MemberAdminService>();

        var result = await service.SearchAsync(null, 1, 20);

        Assert.Equal(2, result.TotalCount);
        Assert.DoesNotContain(result.Items, m => m.DisplayName is "همکار" or "مدیر");
    }

    [Theory]
    [InlineData("علی", 1)]          // نام نمایشی
    [InlineData("ali@", 1)]         // ایمیل
    [InlineData("0912", 1)]         // بخشی از شماره
    [InlineData("۰۹۱۲", 1)]         // ارقام فارسی
    [InlineData("٠٩٣٥", 1)]         // ارقام عربی
    [InlineData("0935123", 1)]
    [InlineData("nobody", 0)]
    [InlineData("staff@", 0)]       // همکار در این فهرست نیست
    [InlineData("boss@", 0)]        // مدیر کل هم نیست
    public async Task Search_MatchesNameEmailAndPhone(string query, int expectedCount)
    {
        using var host = await HostWithPeopleAsync();
        using var scope = host.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<MemberAdminService>();

        var result = await service.SearchAsync(query, 1, 20);

        Assert.Equal(expectedCount, result.TotalCount);
    }

    [Fact]
    public async Task Search_Paginates_NewestFirst()
    {
        using var host = await AdminTestHost.CreateAsync();
        for (var i = 1; i <= 25; i++)
            await host.CreateUserAsync($"user{i:00}@example.com", RoleNames.Member, $"کاربر{i}", email: $"user{i:00}@example.com");
        using var scope = host.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<MemberAdminService>();

        var first = await service.SearchAsync(null, 1, 10);
        var third = await service.SearchAsync(null, 3, 10);
        var beyond = await service.SearchAsync(null, 4, 10);

        Assert.Equal(25, first.TotalCount);
        Assert.Equal(3, first.TotalPages);
        Assert.Equal(10, first.Items.Count);
        Assert.False(first.HasPrevious);
        Assert.True(first.HasNext);
        Assert.Equal(5, third.Items.Count);
        Assert.False(third.HasNext);
        Assert.Empty(beyond.Items);
        Assert.Equal("کاربر25", first.Items[0].DisplayName); // تازه‌ترین اول
    }

    [Fact]
    public async Task Search_ClampsOddPageNumbers()
    {
        using var host = await HostWithPeopleAsync();
        using var scope = host.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<MemberAdminService>();

        var result = await service.SearchAsync(null, -5, 0);

        Assert.Equal(1, result.Page);
        Assert.True(result.PageSize >= 1);
    }

    [Fact]
    public async Task SetActive_DeactivatesMember_AndRotatesTheSecurityStamp()
    {
        using var host = await AdminTestHost.CreateAsync();
        var id = await host.CreateUserAsync("m@example.com", RoleNames.Member, "عضو", email: "m@example.com");
        using var scope = host.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<MemberAdminService>();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var before = await users.GetSecurityStampAsync((await users.FindByIdAsync(id.ToString()))!);

        var result = await service.SetActiveAsync(id, false, Actor);

        Assert.True(result.Succeeded);
        var user = await users.FindByIdAsync(id.ToString());
        Assert.False(user!.IsActive);
        Assert.NotEqual(before, await users.GetSecurityStampAsync(user));
        Assert.Contains(host.Logs.Messages, m => m.Contains("deactivated"));
    }

    [Fact]
    public async Task SetActive_ReactivatesMember()
    {
        using var host = await AdminTestHost.CreateAsync();
        var id = await host.CreateUserAsync("m@example.com", RoleNames.Member, "عضو", email: "m@example.com", isActive: false);
        using var scope = host.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<MemberAdminService>();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

        Assert.True((await service.SetActiveAsync(id, true, Actor)).Succeeded);

        Assert.True((await users.FindByIdAsync(id.ToString()))!.IsActive);
    }

    [Fact]
    public async Task SetActive_RefusesStaffAndSuperAdmin_AndReportsUnknownIds()
    {
        using var host = await HostWithPeopleAsync();
        using var scope = host.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<MemberAdminService>();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var staff = await users.FindByEmailAsync("staff@example.com");
        var boss = await users.FindByEmailAsync("boss@example.com");

        Assert.Equal(AdminOperationError.NotAllowed, (await service.SetActiveAsync(staff!.Id, false, Actor)).Error);
        Assert.Equal(AdminOperationError.NotAllowed, (await service.SetActiveAsync(boss!.Id, false, Actor)).Error);
        Assert.Equal(AdminOperationError.NotFound, (await service.SetActiveAsync(9999, false, Actor)).Error);
        Assert.True(staff.IsActive);
        Assert.True(boss.IsActive);
    }
}
