using MerajAndisheh.Core.Domain;
using MerajAndisheh.Infrastructure.Administration;
using MerajAndisheh.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MerajAndisheh.Tests;

/// <summary>
/// کلیدهای دسترسی هنگام ورود در Claim های کوکی می‌نشینند؛ پس تغییر دسترسی همکار پس از ورود مجدد اعمال می‌شود
/// (معیار تکمیل مرحله‌ی ۶).
/// </summary>
public class PermissionClaimsTests
{
    private static async Task<IReadOnlyList<string>> PermissionClaimsAsync(IServiceScope scope, int userId)
    {
        var users = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var factory = scope.ServiceProvider.GetRequiredService<IUserClaimsPrincipalFactory<AppUser>>();

        var user = await users.FindByIdAsync(userId.ToString());
        var principal = await factory.CreateAsync(user!);

        return principal.FindAll(AppClaimTypes.Permission).Select(c => c.Value).OrderBy(v => v, StringComparer.Ordinal).ToList();
    }

    [Fact]
    public async Task Staff_GetsOneClaimPerGrantedKey()
    {
        using var host = await AdminTestHost.CreateAsync();
        using var scope = host.CreateScope();
        var staff = scope.ServiceProvider.GetRequiredService<StaffService>();
        var created = await staff.CreateAsync(new CreateStaffRequest("s@example.com", "همکار", [PermissionKeys.Users, PermissionKeys.Settings]), 1);

        var claims = await PermissionClaimsAsync(scope, created.UserId!.Value);

        Assert.Equal(new[] { PermissionKeys.Settings, PermissionKeys.Users }, claims);
    }

    [Fact]
    public async Task ChangedPermissions_AppearInTheNextSignIn()
    {
        using var host = await AdminTestHost.CreateAsync();
        using var scope = host.CreateScope();
        var staff = scope.ServiceProvider.GetRequiredService<StaffService>();
        var id = (await staff.CreateAsync(new CreateStaffRequest("s@example.com", "همکار", [PermissionKeys.Users]), 1)).UserId!.Value;

        await staff.UpdatePermissionsAsync(id, [PermissionKeys.Content], 1);
        var claims = await PermissionClaimsAsync(scope, id);

        Assert.Equal(new[] { PermissionKeys.Content }, claims);
    }

    [Fact]
    public async Task SuperAdmin_AndMember_GetNoPermissionClaims()
    {
        using var host = await AdminTestHost.CreateAsync();
        var bossId = await host.CreateUserAsync("b@example.com", RoleNames.SuperAdmin, email: "b@example.com");
        var memberId = await host.CreateUserAsync("m@example.com", RoleNames.Member, email: "m@example.com");
        using var scope = host.CreateScope();

        Assert.Empty(await PermissionClaimsAsync(scope, bossId));
        Assert.Empty(await PermissionClaimsAsync(scope, memberId));
    }

    [Fact]
    public async Task Principal_KeepsRoleAndExistingClaims()
    {
        using var host = await AdminTestHost.CreateAsync();
        using var scope = host.CreateScope();
        var staff = scope.ServiceProvider.GetRequiredService<StaffService>();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var factory = scope.ServiceProvider.GetRequiredService<IUserClaimsPrincipalFactory<AppUser>>();
        var id = (await staff.CreateAsync(new CreateStaffRequest("s@example.com", "نام همکار", []), 1)).UserId!.Value;

        var principal = await factory.CreateAsync((await users.FindByIdAsync(id.ToString()))!);

        Assert.True(principal.IsInRole(RoleNames.Staff));
        Assert.Equal("نام همکار", principal.FindFirst(AppClaimTypes.DisplayName)?.Value);
        Assert.Equal("true", principal.FindFirst(AppClaimTypes.MustChangePassword)?.Value);
    }
}
