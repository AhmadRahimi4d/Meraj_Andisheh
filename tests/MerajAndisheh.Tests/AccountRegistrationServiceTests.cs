using MerajAndisheh.Core.Domain;
using MerajAndisheh.Infrastructure.Data;
using MerajAndisheh.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MerajAndisheh.Tests;

/// <summary>معیارهای مرحله‌ی ۴: ثبت‌نام با ایمیل، تأیید ایمیل و قواعد رمز (بخش ۳.۱۴).</summary>
public class AccountRegistrationServiceTests
{
    private const string ValidPassword = "Passw0rd-OK";

    private static RegistrationRequest Request(string email, string? password, string? displayName, int? ageGroupId)
        => new(email, password, displayName, ageGroupId);

    [Fact]
    public async Task Register_ValidRequest_CreatesMemberWithUnconfirmedEmailAndToken()
    {
        using var host = await IdentityTestHost.CreateAsync();
        using var scope = host.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<AccountRegistrationService>();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var teen = await host.GetAgeGroupIdAsync("teen");

        var result = await service.RegisterAsync(Request("ali@example.com", ValidPassword, "  علی  ", teen));

        Assert.True(result.Succeeded);
        Assert.False(string.IsNullOrWhiteSpace(result.ConfirmationToken));

        var user = await users.FindByEmailAsync("ali@example.com");
        Assert.NotNull(user);
        Assert.False(user!.EmailConfirmed);
        Assert.Equal("علی", user.DisplayName);
        Assert.Equal(teen, user.AgeGroupId);
        Assert.True(user.IsActive);
        Assert.True(await users.IsInRoleAsync(user, RoleNames.Member));
        Assert.NotNull(user.PasswordHash);
        Assert.NotEqual(ValidPassword, user.PasswordHash);
    }

    [Fact]
    public async Task Register_PasswordOf7Chars_FailsWithPasswordTooShort_And_8CharsSucceeds()
    {
        using var host = await IdentityTestHost.CreateAsync();
        using var scope = host.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<AccountRegistrationService>();
        var adult = await host.GetAgeGroupIdAsync("adult");

        var tooShort = await service.RegisterAsync(Request("a@example.com", "1234567", "الف", adult));
        Assert.False(tooShort.Succeeded);
        Assert.Contains(RegistrationError.PasswordTooShort, tooShort.Errors);

        var exact = await service.RegisterAsync(Request("a@example.com", "12345678", "الف", adult));
        Assert.True(exact.Succeeded);
    }

    [Fact]
    public async Task Register_DuplicateEmail_IsRejected_CaseInsensitively()
    {
        using var host = await IdentityTestHost.CreateAsync();
        using var scope = host.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<AccountRegistrationService>();
        var youth = await host.GetAgeGroupIdAsync("youth");

        await service.RegisterAsync(Request("sara@example.com", ValidPassword, "سارا", youth));
        var second = await service.RegisterAsync(Request("SARA@Example.com", ValidPassword, "سارا دو", youth));

        Assert.False(second.Succeeded);
        Assert.Contains(RegistrationError.EmailTaken, second.Errors);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    [InlineData("a@")]
    public async Task Register_InvalidEmail_IsRejected(string email)
    {
        using var host = await IdentityTestHost.CreateAsync();
        using var scope = host.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<AccountRegistrationService>();
        var adult = await host.GetAgeGroupIdAsync("adult");

        var result = await service.RegisterAsync(Request(email, ValidPassword, "الف", adult));

        Assert.False(result.Succeeded);
        Assert.Contains(RegistrationError.EmailInvalid, result.Errors);
    }

    [Fact]
    public async Task Register_DisplayName_EmptyOrOverLimit_IsRejected()
    {
        using var host = await IdentityTestHost.CreateAsync();
        using var scope = host.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<AccountRegistrationService>();
        var adult = await host.GetAgeGroupIdAsync("adult");

        var empty = await service.RegisterAsync(Request("e@example.com", ValidPassword, "   ", adult));
        Assert.Contains(RegistrationError.DisplayNameInvalid, empty.Errors);

        var tooLong = await service.RegisterAsync(Request("l@example.com", ValidPassword,
            new string('ا', AccountRules.DisplayNameMaxLength + 1), adult));
        Assert.Contains(RegistrationError.DisplayNameInvalid, tooLong.Errors);

        var exact = await service.RegisterAsync(Request("x@example.com", ValidPassword,
            new string('ا', AccountRules.DisplayNameMaxLength), adult));
        Assert.True(exact.Succeeded);
    }

    [Fact]
    public async Task Register_AgeGroup_MustExistAndBeActive()
    {
        using var host = await IdentityTestHost.CreateAsync();
        using var scope = host.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<AccountRegistrationService>();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var missing = await service.RegisterAsync(Request("m@example.com", ValidPassword, "الف", null));
        Assert.Contains(RegistrationError.AgeGroupInvalid, missing.Errors);

        var unknown = await service.RegisterAsync(Request("u@example.com", ValidPassword, "الف", 9999));
        Assert.Contains(RegistrationError.AgeGroupInvalid, unknown.Errors);

        var child = await db.AgeGroups.SingleAsync(a => a.Key == "child");
        child.IsActive = false;
        await db.SaveChangesAsync();
        var inactive = await service.RegisterAsync(Request("i@example.com", ValidPassword, "الف", child.Id));
        Assert.Contains(RegistrationError.AgeGroupInvalid, inactive.Errors);
    }

    [Fact]
    public async Task Register_Failure_DoesNotCreateUser()
    {
        using var host = await IdentityTestHost.CreateAsync();
        using var scope = host.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<AccountRegistrationService>();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var adult = await host.GetAgeGroupIdAsync("adult");

        await service.RegisterAsync(Request("no@example.com", "short", "الف", adult));

        Assert.Null(await users.FindByEmailAsync("no@example.com"));
    }

    [Fact]
    public async Task ConfirmEmail_WithRealToken_ConfirmsAccount_AndWrongTokenFails()
    {
        using var host = await IdentityTestHost.CreateAsync();
        using var scope = host.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<AccountRegistrationService>();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var adult = await host.GetAgeGroupIdAsync("adult");

        var result = await service.RegisterAsync(Request("c@example.com", ValidPassword, "الف", adult));
        var user = result.User!;

        var wrong = await users.ConfirmEmailAsync(user, "not-a-valid-token");
        Assert.False(wrong.Succeeded);
        Assert.False(await users.IsEmailConfirmedAsync(user));

        var ok = await users.ConfirmEmailAsync(user, result.ConfirmationToken!);
        Assert.True(ok.Succeeded);
        Assert.True(await users.IsEmailConfirmedAsync(user));
    }
}
