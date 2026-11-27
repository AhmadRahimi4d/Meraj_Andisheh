using MerajAndisheh.Core.Domain;
using MerajAndisheh.Infrastructure.Administration;
using MerajAndisheh.Infrastructure.Data;
using MerajAndisheh.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MerajAndisheh.Tests;

/// <summary>ساخت و مدیریت حساب همکار توسط مدیر کل (مرحله‌ی ۶).</summary>
public class StaffServiceTests
{
    private const int Actor = 1;

    private static CreateStaffRequest Request(string email = "staff@example.com", string name = "همکار", params string[] permissions)
        => new(email, name, permissions);

    [Fact]
    public async Task Create_MakesStaffAccount_WithTemporaryPassword_AndPermissions()
    {
        using var host = await AdminTestHost.CreateAsync();
        using var scope = host.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<StaffService>();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var result = await service.CreateAsync(Request(permissions: [PermissionKeys.Users, PermissionKeys.Content]), Actor);

        Assert.True(result.Succeeded);
        Assert.False(string.IsNullOrEmpty(result.TemporaryPassword));

        var user = await users.FindByEmailAsync("staff@example.com");
        Assert.NotNull(user);
        Assert.True(await users.IsInRoleAsync(user, RoleNames.Staff));
        Assert.False(await users.IsInRoleAsync(user, RoleNames.Member));
        Assert.False(await users.IsInRoleAsync(user, RoleNames.SuperAdmin));
        Assert.True(user.EmailConfirmed);
        Assert.True(user.MustChangePassword);
        Assert.True(user.IsActive);
        Assert.True(await users.CheckPasswordAsync(user, result.TemporaryPassword!));

        var keys = await db.StaffPermissions.Where(p => p.UserId == user.Id).Select(p => p.PermissionKey).ToListAsync();
        Assert.Equal(new[] { PermissionKeys.Content, PermissionKeys.Users }.OrderBy(k => k), keys.OrderBy(k => k));
    }

    [Fact]
    public async Task Create_DoesNotStoreThePlainPassword()
    {
        using var host = await AdminTestHost.CreateAsync();
        using var scope = host.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<StaffService>();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

        var result = await service.CreateAsync(Request(), Actor);
        var user = await users.FindByEmailAsync("staff@example.com");

        Assert.NotNull(user!.PasswordHash);
        Assert.DoesNotContain(result.TemporaryPassword!, user.PasswordHash);
        Assert.DoesNotContain(host.Logs.Messages, m => m.Contains(result.TemporaryPassword!));
    }

    [Fact]
    public async Task Create_WithoutPermissions_IsAllowed()
    {
        using var host = await AdminTestHost.CreateAsync();
        using var scope = host.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<StaffService>();

        var result = await service.CreateAsync(Request(), Actor);
        var row = await service.GetAsync(result.UserId!.Value);

        Assert.True(result.Succeeded);
        Assert.Empty(row!.Permissions);
    }

    [Theory]
    [InlineData("", "همکار", StaffError.EmailInvalid)]
    [InlineData("not-an-email", "همکار", StaffError.EmailInvalid)]
    [InlineData("a@example.com", "", StaffError.DisplayNameInvalid)]
    [InlineData("a@example.com", "   ", StaffError.DisplayNameInvalid)]
    public async Task Create_InvalidInput_IsRejected_AndNothingIsCreated(string email, string name, StaffError expected)
    {
        using var host = await AdminTestHost.CreateAsync();
        using var scope = host.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<StaffService>();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

        var result = await service.CreateAsync(Request(email, name), Actor);

        Assert.False(result.Succeeded);
        Assert.Contains(expected, result.Errors);
        Assert.Null(result.TemporaryPassword);
        Assert.Equal(0, await users.Users.CountAsync());
    }

    [Fact]
    public async Task Create_DisplayNameOverTheLimit_IsRejected()
    {
        using var host = await AdminTestHost.CreateAsync();
        using var scope = host.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<StaffService>();

        var result = await service.CreateAsync(Request(name: new string('ا', AccountRules.DisplayNameMaxLength + 1)), Actor);

        Assert.Contains(StaffError.DisplayNameInvalid, result.Errors);
    }

    [Fact]
    public async Task Create_UnknownPermissionKey_IsRejected()
    {
        using var host = await AdminTestHost.CreateAsync();
        using var scope = host.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<StaffService>();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

        var result = await service.CreateAsync(Request(permissions: [PermissionKeys.Users, "content.delete"]), Actor);

        Assert.Contains(StaffError.PermissionInvalid, result.Errors);
        Assert.Equal(0, await users.Users.CountAsync());
    }

    [Fact]
    public async Task Create_DuplicateEmail_IsRejected_WhateverTheCase()
    {
        using var host = await AdminTestHost.CreateAsync();
        using var scope = host.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<StaffService>();

        Assert.True((await service.CreateAsync(Request("same@example.com"), Actor)).Succeeded);
        var second = await service.CreateAsync(Request("SAME@Example.com"), Actor);

        Assert.False(second.Succeeded);
        Assert.Contains(StaffError.EmailTaken, second.Errors);
    }

    [Fact]
    public async Task Create_EmailOfAnExistingMember_IsRejected()
    {
        using var host = await AdminTestHost.CreateAsync();
        await host.CreateUserAsync("member@example.com", RoleNames.Member, email: "member@example.com");
        using var scope = host.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<StaffService>();

        var result = await service.CreateAsync(Request("member@example.com"), Actor);

        Assert.Contains(StaffError.EmailTaken, result.Errors);
    }

    [Fact]
    public async Task List_ReturnsOnlyStaff_WithPermissionsInDisplayOrder()
    {
        using var host = await AdminTestHost.CreateAsync();
        await host.CreateUserAsync("member@example.com", RoleNames.Member, email: "member@example.com");
        await host.CreateUserAsync("boss@example.com", RoleNames.SuperAdmin, email: "boss@example.com");
        using var scope = host.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<StaffService>();
        await service.CreateAsync(Request("s1@example.com", "الف", PermissionKeys.Settings, PermissionKeys.Dashboard), Actor);
        await service.CreateAsync(Request("s2@example.com", "ب"), Actor);

        var rows = await service.ListAsync();

        Assert.Equal(2, rows.Count);
        var first = rows.Single(r => r.Email == "s1@example.com");
        Assert.Equal(new[] { PermissionKeys.Dashboard, PermissionKeys.Settings }, first.Permissions);
    }

    [Fact]
    public async Task Get_ReturnsNull_ForMembersSuperAdminsAndUnknownIds()
    {
        using var host = await AdminTestHost.CreateAsync();
        var memberId = await host.CreateUserAsync("m@example.com", RoleNames.Member, email: "m@example.com");
        var bossId = await host.CreateUserAsync("b@example.com", RoleNames.SuperAdmin, email: "b@example.com");
        using var scope = host.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<StaffService>();

        Assert.Null(await service.GetAsync(memberId));
        Assert.Null(await service.GetAsync(bossId));
        Assert.Null(await service.GetAsync(9999));
    }

    [Fact]
    public async Task UpdatePermissions_ReplacesTheSet_AndRotatesTheSecurityStamp()
    {
        using var host = await AdminTestHost.CreateAsync();
        using var scope = host.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<StaffService>();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var created = await service.CreateAsync(Request(permissions: [PermissionKeys.Users, PermissionKeys.Content]), Actor);
        var id = created.UserId!.Value;
        var before = await users.GetSecurityStampAsync((await users.FindByIdAsync(id.ToString()))!);

        var result = await service.UpdatePermissionsAsync(id, [PermissionKeys.Settings], Actor);

        Assert.True(result.Succeeded);
        var row = await service.GetAsync(id);
        Assert.Equal(new[] { PermissionKeys.Settings }, row!.Permissions);

        var after = await users.GetSecurityStampAsync((await users.FindByIdAsync(id.ToString()))!);
        Assert.NotEqual(before, after);
        Assert.Contains(host.Logs.Messages, m => m.Contains("permissions changed"));
    }

    [Fact]
    public async Task UpdatePermissions_WithOverlappingKeys_KeepsSharedOnesAndAppliesTheDifference()
    {
        using var host = await AdminTestHost.CreateAsync();
        using var scope = host.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<StaffService>();
        var created = await service.CreateAsync(Request(permissions: [PermissionKeys.Users, PermissionKeys.Content]), Actor);
        var id = created.UserId!.Value;

        var result = await service.UpdatePermissionsAsync(id, [PermissionKeys.Users, PermissionKeys.Settings], Actor);

        Assert.True(result.Succeeded);
        Assert.Equal(new[] { PermissionKeys.Users, PermissionKeys.Settings }, (await service.GetAsync(id))!.Permissions);
    }

    [Fact]
    public async Task UpdatePermissions_CanRemoveEverything()
    {
        using var host = await AdminTestHost.CreateAsync();
        using var scope = host.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<StaffService>();
        var created = await service.CreateAsync(Request(permissions: [PermissionKeys.Users]), Actor);

        var result = await service.UpdatePermissionsAsync(created.UserId!.Value, null, Actor);

        Assert.True(result.Succeeded);
        Assert.Empty((await service.GetAsync(created.UserId.Value))!.Permissions);
    }

    [Fact]
    public async Task UpdatePermissions_InvalidKey_ChangesNothing()
    {
        using var host = await AdminTestHost.CreateAsync();
        using var scope = host.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<StaffService>();
        var created = await service.CreateAsync(Request(permissions: [PermissionKeys.Users]), Actor);

        var result = await service.UpdatePermissionsAsync(created.UserId!.Value, [PermissionKeys.Content, "bogus"], Actor);

        Assert.Equal(AdminOperationError.PermissionInvalid, result.Error);
        Assert.Equal(new[] { PermissionKeys.Users }, (await service.GetAsync(created.UserId.Value))!.Permissions);
    }

    [Fact]
    public async Task UpdatePermissions_ForANonStaffAccount_IsNotFound()
    {
        using var host = await AdminTestHost.CreateAsync();
        var memberId = await host.CreateUserAsync("m@example.com", RoleNames.Member, email: "m@example.com");
        using var scope = host.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<StaffService>();

        Assert.Equal(AdminOperationError.NotFound, (await service.UpdatePermissionsAsync(memberId, [PermissionKeys.Users], Actor)).Error);
        Assert.Equal(AdminOperationError.NotFound, (await service.UpdatePermissionsAsync(9999, [], Actor)).Error);
    }

    [Fact]
    public async Task SetActive_DeactivatesAndReactivates_AndRotatesStampOnDeactivation()
    {
        using var host = await AdminTestHost.CreateAsync();
        using var scope = host.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<StaffService>();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var id = (await service.CreateAsync(Request(), Actor)).UserId!.Value;
        var before = await users.GetSecurityStampAsync((await users.FindByIdAsync(id.ToString()))!);

        Assert.True((await service.SetActiveAsync(id, false, Actor)).Succeeded);
        var deactivated = await users.FindByIdAsync(id.ToString());
        Assert.False(deactivated!.IsActive);
        Assert.NotEqual(before, await users.GetSecurityStampAsync(deactivated));

        Assert.True((await service.SetActiveAsync(id, true, Actor)).Succeeded);
        Assert.True((await users.FindByIdAsync(id.ToString()))!.IsActive);
    }

    [Fact]
    public async Task SetActive_DoesNotTouchMembersOrSuperAdmins()
    {
        using var host = await AdminTestHost.CreateAsync();
        var memberId = await host.CreateUserAsync("m@example.com", RoleNames.Member, email: "m@example.com");
        var bossId = await host.CreateUserAsync("b@example.com", RoleNames.SuperAdmin, email: "b@example.com");
        using var scope = host.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<StaffService>();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

        Assert.Equal(AdminOperationError.NotFound, (await service.SetActiveAsync(memberId, false, Actor)).Error);
        Assert.Equal(AdminOperationError.NotFound, (await service.SetActiveAsync(bossId, false, Actor)).Error);
        Assert.True((await users.FindByIdAsync(memberId.ToString()))!.IsActive);
        Assert.True((await users.FindByIdAsync(bossId.ToString()))!.IsActive);
    }
}
