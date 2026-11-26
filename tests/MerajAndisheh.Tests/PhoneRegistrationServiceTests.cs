using MerajAndisheh.Core.Domain;
using MerajAndisheh.Infrastructure.Data;
using MerajAndisheh.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MerajAndisheh.Tests;

/// <summary>ثبت‌نام با موبایل: حساب Member بدون ایمیل و بدون رمز، با شماره‌ی تأییدشده.</summary>
public class PhoneRegistrationServiceTests
{
    private const string Phone = "09121234567";

    private static PhoneRegistrationRequest Request(string? phone, string? displayName, int? ageGroupId)
        => new(phone, displayName, ageGroupId);

    [Fact]
    public async Task Register_ValidRequest_CreatesConfirmedMemberWithoutEmailOrPassword()
    {
        using var host = await IdentityTestHost.CreateAsync();
        using var scope = host.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<PhoneRegistrationService>();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var teen = await host.GetAgeGroupIdAsync("teen");

        var result = await service.RegisterAsync(Request(Phone, "  علی  ", teen));

        Assert.True(result.Succeeded);
        var user = await users.Users.SingleAsync(u => u.PhoneNumber == Phone);
        Assert.Equal(Phone, user.UserName);
        Assert.Null(user.Email);
        Assert.True(user.PhoneNumberConfirmed);
        Assert.Null(user.PasswordHash);
        Assert.Equal("علی", user.DisplayName);
        Assert.Equal(teen, user.AgeGroupId);
        Assert.True(user.IsActive);
        Assert.False(user.MustChangePassword);
        Assert.True(await users.IsInRoleAsync(user, RoleNames.Member));
    }

    [Fact]
    public async Task Register_NormalizesThePhoneNumber()
    {
        using var host = await IdentityTestHost.CreateAsync();
        using var scope = host.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<PhoneRegistrationService>();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var adult = await host.GetAgeGroupIdAsync("adult");

        var result = await service.RegisterAsync(Request("+98 ۹۱۲ ۱۲۳ ۴۵۶۷", "سارا", adult));

        Assert.True(result.Succeeded);
        Assert.Equal(Phone, result.User!.PhoneNumber);
        Assert.NotNull(await users.Users.SingleOrDefaultAsync(u => u.PhoneNumber == Phone));
    }

    [Fact]
    public async Task Register_SamePhoneTwice_IsRejected_InAnyFormat()
    {
        using var host = await IdentityTestHost.CreateAsync();
        using var scope = host.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<PhoneRegistrationService>();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var adult = await host.GetAgeGroupIdAsync("adult");

        Assert.True((await service.RegisterAsync(Request(Phone, "الف", adult))).Succeeded);
        var second = await service.RegisterAsync(Request("۰۹۱۲۱۲۳۴۵۶۷", "ب", adult));

        Assert.False(second.Succeeded);
        Assert.Contains(RegistrationError.PhoneTaken, second.Errors);
        Assert.Equal(1, await users.Users.CountAsync());
    }

    [Fact]
    public async Task Register_TwoPhoneAccounts_BothWithoutEmail_AreAllowed()
    {
        using var host = await IdentityTestHost.CreateAsync();
        using var scope = host.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<PhoneRegistrationService>();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var adult = await host.GetAgeGroupIdAsync("adult");

        var first = await service.RegisterAsync(Request("09121234567", "الف", adult));
        var second = await service.RegisterAsync(Request("09351234567", "ب", adult));

        Assert.True(first.Succeeded);
        Assert.True(second.Succeeded);
        Assert.Equal(2, await users.Users.CountAsync());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("09123")]
    [InlineData("ali@example.com")]
    [InlineData("02112345678")]
    public async Task Register_InvalidPhone_IsRejected(string? phone)
    {
        using var host = await IdentityTestHost.CreateAsync();
        using var scope = host.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<PhoneRegistrationService>();
        var adult = await host.GetAgeGroupIdAsync("adult");

        var result = await service.RegisterAsync(Request(phone, "الف", adult));

        Assert.False(result.Succeeded);
        Assert.Contains(RegistrationError.PhoneInvalid, result.Errors);
    }

    [Fact]
    public async Task Register_DisplayName_EmptyOrOverLimit_IsRejected()
    {
        using var host = await IdentityTestHost.CreateAsync();
        using var scope = host.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<PhoneRegistrationService>();
        var adult = await host.GetAgeGroupIdAsync("adult");

        var empty = await service.RegisterAsync(Request(Phone, "   ", adult));
        Assert.Contains(RegistrationError.DisplayNameInvalid, empty.Errors);

        var tooLong = await service.RegisterAsync(Request(Phone, new string('ا', AccountRules.DisplayNameMaxLength + 1), adult));
        Assert.Contains(RegistrationError.DisplayNameInvalid, tooLong.Errors);

        var exact = await service.RegisterAsync(Request(Phone, new string('ا', AccountRules.DisplayNameMaxLength), adult));
        Assert.True(exact.Succeeded);
    }

    [Fact]
    public async Task Register_AgeGroup_MustExistAndBeActive()
    {
        using var host = await IdentityTestHost.CreateAsync();
        using var scope = host.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<PhoneRegistrationService>();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        Assert.Contains(RegistrationError.AgeGroupInvalid, (await service.RegisterAsync(Request(Phone, "الف", null))).Errors);
        Assert.Contains(RegistrationError.AgeGroupInvalid, (await service.RegisterAsync(Request(Phone, "الف", 9999))).Errors);

        var child = await db.AgeGroups.SingleAsync(a => a.Key == "child");
        child.IsActive = false;
        await db.SaveChangesAsync();
        Assert.Contains(RegistrationError.AgeGroupInvalid, (await service.RegisterAsync(Request(Phone, "الف", child.Id))).Errors);
    }

    [Fact]
    public async Task Register_Failure_DoesNotCreateUser()
    {
        using var host = await IdentityTestHost.CreateAsync();
        using var scope = host.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<PhoneRegistrationService>();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

        await service.RegisterAsync(Request("09123", "الف", null));

        Assert.Equal(0, await users.Users.CountAsync());
    }
}
