using MerajAndisheh.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MerajAndisheh.Tests;

/// <summary>
/// RequireUniqueEmail خاموش شد (تا کاربر بدون ایمیل ساخته شود)؛ این تست‌ها ثابت می‌کنند که یکتایی و قالب ایمیل
/// و یکتایی شماره هنوز اعمال می‌شود.
/// </summary>
public class AppUserValidatorTests
{
    private static AppUser NewUser(string userName, string? email = null, string? phone = null) => new()
    {
        UserName = userName,
        Email = email,
        PhoneNumber = phone,
        DisplayName = "تست",
        CreatedAt = DateTime.UtcNow
    };

    [Fact]
    public async Task DuplicateEmail_IsRejected_CaseInsensitively()
    {
        using var host = await IdentityTestHost.CreateAsync();
        using var scope = host.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

        Assert.True((await users.CreateAsync(NewUser("a@example.com", "a@example.com"), "Passw0rd-OK")).Succeeded);
        var second = await users.CreateAsync(NewUser("b", "A@Example.com"), "Passw0rd-OK");

        Assert.False(second.Succeeded);
        Assert.Contains(second.Errors, e => e.Code == "DuplicateEmail");
    }

    [Fact]
    public async Task InvalidEmailFormat_IsRejected()
    {
        using var host = await IdentityTestHost.CreateAsync();
        using var scope = host.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

        var result = await users.CreateAsync(NewUser("x", "not-an-email"), "Passw0rd-OK");

        Assert.False(result.Succeeded);
        Assert.Contains(result.Errors, e => e.Code == "InvalidEmail");
    }

    [Fact]
    public async Task MissingEmail_IsAllowed()
    {
        using var host = await IdentityTestHost.CreateAsync();
        using var scope = host.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

        Assert.True((await users.CreateAsync(NewUser("09121234567", null, "09121234567"))).Succeeded);
        Assert.True((await users.CreateAsync(NewUser("09351234567", null, "09351234567"))).Succeeded);
    }

    [Fact]
    public async Task DuplicatePhoneNumber_IsRejected()
    {
        using var host = await IdentityTestHost.CreateAsync();
        using var scope = host.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

        Assert.True((await users.CreateAsync(NewUser("first", null, "09121234567"))).Succeeded);
        var second = await users.CreateAsync(NewUser("second", null, "09121234567"));

        Assert.False(second.Succeeded);
        Assert.Contains(second.Errors, e => e.Code == AppUserValidator.DuplicatePhoneNumberCode);
    }

    [Fact]
    public async Task UpdatingAnExistingUser_IsNotFlaggedAsItsOwnDuplicate()
    {
        using var host = await IdentityTestHost.CreateAsync();
        using var scope = host.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var user = NewUser("a@example.com", "a@example.com", "09121234567");
        Assert.True((await users.CreateAsync(user, "Passw0rd-OK")).Succeeded);

        user.DisplayName = "نام تازه";
        var update = await users.UpdateAsync(user);

        Assert.True(update.Succeeded);
    }
}
