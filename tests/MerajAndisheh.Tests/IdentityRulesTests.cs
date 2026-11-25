using MerajAndisheh.Core.Domain;
using MerajAndisheh.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace MerajAndisheh.Tests;

/// <summary>قاعده‌های ورود (بخش ۳.۱۴): رمز ۸ نویسه، ۵ ورود ناموفق ← قفل ۱۵ دقیقه‌ای، تأیید ایمیل.</summary>
public class IdentityRulesTests
{
    [Fact]
    public async Task Options_MatchDesignDoc()
    {
        using var host = await IdentityTestHost.CreateAsync();
        var options = host.Services.GetRequiredService<IOptions<IdentityOptions>>().Value;

        Assert.Equal(8, options.Password.RequiredLength);
        Assert.Equal(5, options.Lockout.MaxFailedAccessAttempts);
        Assert.Equal(TimeSpan.FromMinutes(15), options.Lockout.DefaultLockoutTimeSpan);
        Assert.True(options.SignIn.RequireConfirmedEmail);
        Assert.True(options.User.RequireUniqueEmail);
        Assert.True(options.Lockout.AllowedForNewUsers);
    }

    [Fact]
    public async Task FourFailedAttempts_DoNotLockTheAccount()
    {
        using var host = await IdentityTestHost.CreateAsync();
        using var scope = host.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var user = await CreateUserAsync(users, "lock4@example.com");

        for (var i = 0; i < AccountRules.MaxFailedAttempts - 1; i++)
        {
            await users.AccessFailedAsync(user);
        }

        Assert.False(await users.IsLockedOutAsync(user));
    }

    [Fact]
    public async Task FiveFailedAttempts_LockTheAccountForAboutFifteenMinutes()
    {
        using var host = await IdentityTestHost.CreateAsync();
        using var scope = host.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var user = await CreateUserAsync(users, "lock5@example.com");

        for (var i = 0; i < AccountRules.MaxFailedAttempts; i++)
        {
            await users.AccessFailedAsync(user);
        }

        Assert.True(await users.IsLockedOutAsync(user));

        var lockoutEnd = await users.GetLockoutEndDateAsync(user);
        Assert.NotNull(lockoutEnd);
        var remaining = lockoutEnd!.Value - DateTimeOffset.UtcNow;
        Assert.InRange(remaining.TotalMinutes, AccountRules.LockoutMinutes - 1, AccountRules.LockoutMinutes + 0.1);
    }

    private static async Task<AppUser> CreateUserAsync(UserManager<AppUser> users, string email)
    {
        var user = new AppUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            DisplayName = "تست",
            CreatedAt = DateTime.UtcNow
        };

        var result = await users.CreateAsync(user, "Passw0rd-OK");
        Assert.True(result.Succeeded);
        return user;
    }
}
