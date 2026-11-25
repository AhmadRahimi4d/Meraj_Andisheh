using MerajAndisheh.Core.Domain;
using MerajAndisheh.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MerajAndisheh.Tests;

/// <summary>معیارهای مرحله‌ی ۴: نقش‌ها و مدیر کل اولیه (بخش ۳.۱۵) بدون رمز ثابت در کد.</summary>
public class IdentitySeederTests
{
    private const string AdminEmail = "root@example.com";
    private const string TemporaryPassword = "Temp-Secret-1";

    [Fact]
    public async Task SeedRoles_CreatesThreeRoles_AndIsRepeatable()
    {
        using var host = await IdentityTestHost.CreateAsync(seedRoles: false);
        using var scope = host.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<IdentitySeeder>();
        var roles = scope.ServiceProvider.GetRequiredService<RoleManager<AppRole>>();

        await seeder.SeedRolesAsync();
        await seeder.SeedRolesAsync();

        Assert.Equal(3, await roles.Roles.CountAsync());
        foreach (var name in RoleNames.All)
            Assert.True(await roles.RoleExistsAsync(name));
    }

    [Fact]
    public async Task SeedSuperAdmin_CreatesConfirmedAccount_WithMustChangePassword()
    {
        using var host = await IdentityTestHost.CreateAsync(seedRoles: false);
        using var scope = host.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<IdentitySeeder>();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

        var result = await seeder.SeedSuperAdminAsync(AdminEmail, TemporaryPassword);

        Assert.Equal(SuperAdminSeedResult.Created, result);

        var admin = await users.FindByEmailAsync(AdminEmail);
        Assert.NotNull(admin);
        Assert.True(admin!.MustChangePassword);
        Assert.True(admin.EmailConfirmed);
        Assert.True(admin.IsActive);
        Assert.True(await users.IsInRoleAsync(admin, RoleNames.SuperAdmin));
        Assert.True(await users.CheckPasswordAsync(admin, TemporaryPassword));
        Assert.NotEqual(TemporaryPassword, admin.PasswordHash);
    }

    [Fact]
    public async Task SeedSuperAdmin_RunTwice_CreatesOnlyOneAdmin()
    {
        using var host = await IdentityTestHost.CreateAsync();
        using var scope = host.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<IdentitySeeder>();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

        var first = await seeder.SeedSuperAdminAsync(AdminEmail, TemporaryPassword);
        var second = await seeder.SeedSuperAdminAsync(AdminEmail, TemporaryPassword);

        Assert.Equal(SuperAdminSeedResult.Created, first);
        Assert.Equal(SuperAdminSeedResult.AlreadyExists, second);
        Assert.Equal(1, await users.Users.CountAsync());
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("root@example.com", null)]
    [InlineData("root@example.com", "")]
    [InlineData("", "Temp-Secret-1")]
    [InlineData("   ", "Temp-Secret-1")]
    public async Task SeedSuperAdmin_WithoutSecrets_CreatesNothing(string? email, string? password)
    {
        using var host = await IdentityTestHost.CreateAsync();
        using var scope = host.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<IdentitySeeder>();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

        var result = await seeder.SeedSuperAdminAsync(email, password);

        Assert.Equal(SuperAdminSeedResult.NotConfigured, result);
        Assert.Equal(0, await users.Users.CountAsync());
    }

    [Fact]
    public async Task SeedSuperAdmin_WithTooShortPassword_FailsAndCreatesNothing()
    {
        using var host = await IdentityTestHost.CreateAsync();
        using var scope = host.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<IdentitySeeder>();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

        var result = await seeder.SeedSuperAdminAsync(AdminEmail, "short");

        Assert.Equal(SuperAdminSeedResult.Failed, result);
        Assert.Equal(0, await users.Users.CountAsync());
    }
}
