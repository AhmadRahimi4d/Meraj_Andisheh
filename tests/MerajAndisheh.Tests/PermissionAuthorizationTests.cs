using System.Security.Claims;
using MerajAndisheh.Core.Domain;
using MerajAndisheh.Infrastructure.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MerajAndisheh.Tests;

/// <summary>
/// Policy های دسترسی (بخش ۷.۷): مدیر کل همه، همکار فقط کلیدهای خودش، عضو و ورودنکرده هیچ.
/// مدیریت همکاران فقط برای مدیر کل است و با هیچ مجموعه‌ای از کلیدها به همکار واگذار نمی‌شود.
/// </summary>
public class PermissionAuthorizationTests
{
    private static ServiceProvider Build()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAppAuthorization();
        return services.BuildServiceProvider();
    }

    private static ClaimsPrincipal Principal(string? role, params string[] permissions)
    {
        var identity = new ClaimsIdentity("test");
        if (role is not null) identity.AddClaim(new Claim(ClaimTypes.Role, role));
        foreach (var key in permissions) identity.AddClaim(new Claim(AppClaimTypes.Permission, key));
        return new ClaimsPrincipal(identity);
    }

    private static async Task<bool> AllowedAsync(ServiceProvider provider, ClaimsPrincipal user, string policy)
    {
        var authorization = provider.GetRequiredService<IAuthorizationService>();
        return (await authorization.AuthorizeAsync(user, null, policy)).Succeeded;
    }

    [Fact]
    public async Task EveryPermissionKey_HasAPolicyWithTheSameName()
    {
        using var provider = Build();
        var policies = provider.GetRequiredService<IAuthorizationPolicyProvider>();

        foreach (var key in PermissionKeys.All)
            Assert.NotNull(await policies.GetPolicyAsync(key));

        Assert.NotNull(await policies.GetPolicyAsync(PolicyNames.SuperAdminOnly));
        Assert.NotNull(await policies.GetPolicyAsync(PolicyNames.AnyStaff));
    }

    [Fact]
    public async Task SuperAdmin_PassesEveryPolicy_WithoutAnyPermissionClaim()
    {
        using var provider = Build();
        var superAdmin = Principal(RoleNames.SuperAdmin);

        foreach (var key in PermissionKeys.All)
            Assert.True(await AllowedAsync(provider, superAdmin, key), key);

        Assert.True(await AllowedAsync(provider, superAdmin, PolicyNames.SuperAdminOnly));
        Assert.True(await AllowedAsync(provider, superAdmin, PolicyNames.AnyStaff));
    }

    [Fact]
    public async Task Staff_PassesOnlyThePoliciesOfTheirKeys()
    {
        using var provider = Build();
        var staff = Principal(RoleNames.Staff, PermissionKeys.Users, PermissionKeys.Settings);

        Assert.True(await AllowedAsync(provider, staff, PermissionKeys.Users));
        Assert.True(await AllowedAsync(provider, staff, PermissionKeys.Settings));
        Assert.False(await AllowedAsync(provider, staff, PermissionKeys.Content));
        Assert.False(await AllowedAsync(provider, staff, PermissionKeys.Dashboard));
        Assert.True(await AllowedAsync(provider, staff, PolicyNames.AnyStaff));
    }

    [Fact]
    public async Task Staff_WithoutContentKey_IsDeniedContent()
    {
        using var provider = Build();
        var staff = Principal(RoleNames.Staff, PermissionKeys.Users);

        Assert.False(await AllowedAsync(provider, staff, PermissionKeys.Content));
    }

    [Fact]
    public async Task Staff_CannotManageStaff_EvenWithEveryKey()
    {
        using var provider = Build();
        var staff = Principal(RoleNames.Staff, PermissionKeys.All.ToArray());

        Assert.False(await AllowedAsync(provider, staff, PolicyNames.SuperAdminOnly));
    }

    [Fact]
    public async Task Member_IsDeniedEverything_EvenWithForgedPermissionClaims()
    {
        using var provider = Build();
        var member = Principal(RoleNames.Member, PermissionKeys.All.ToArray());

        foreach (var key in PermissionKeys.All)
            Assert.False(await AllowedAsync(provider, member, key), key);

        Assert.False(await AllowedAsync(provider, member, PolicyNames.SuperAdminOnly));
        Assert.False(await AllowedAsync(provider, member, PolicyNames.AnyStaff));
    }

    [Fact]
    public async Task Anonymous_IsDeniedEverything()
    {
        using var provider = Build();
        var anonymous = new ClaimsPrincipal(new ClaimsIdentity());

        foreach (var key in PermissionKeys.All)
            Assert.False(await AllowedAsync(provider, anonymous, key), key);

        Assert.False(await AllowedAsync(provider, anonymous, PolicyNames.AnyStaff));
    }
}
