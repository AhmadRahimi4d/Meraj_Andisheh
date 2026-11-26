using MerajAndisheh.Core.Domain;
using MerajAndisheh.Core.Services;
using MerajAndisheh.Infrastructure.Data;
using MerajAndisheh.Infrastructure.Otp;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MerajAndisheh.Tests;

/// <summary>معیارهای مرحله‌ی ۵ برای OTP: ۶ رقم، ۲ دقیقه، ۵ تلاش، ۶۰ ثانیه، ۵ کد در ساعت، ذخیره‌ی هش.</summary>
public class OtpServiceTests
{
    private const string Phone = "09121234567";
    private const string OtherPhone = "09351234567";

    private static readonly TimeSpan Cooldown = TimeSpan.FromSeconds(OtpRules.ResendCooldownSeconds + 1);

    [Fact]
    public async Task Send_StoresOnlyTheHash_AndSendsThePlainCodeBySms()
    {
        using var host = await OtpTestHost.CreateAsync();
        using var scope = host.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<OtpService>();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        host.Codes.Enqueue("123456");

        var result = await service.SendAsync(Phone);

        Assert.True(result.Sent);
        var sms = Assert.Single(host.Sms.Sent);
        Assert.Equal(Phone, sms.PhoneNumber);
        Assert.Equal("123456", sms.Code);

        var row = await db.OtpCodes.SingleAsync();
        Assert.Equal(Phone, row.PhoneNumber);
        Assert.NotEqual("123456", row.CodeHash);
        Assert.Equal(64, row.CodeHash.Length);
        Assert.Equal(0, row.Attempts);
        Assert.Null(row.UsedAt);
        Assert.Equal(host.Clock.UtcNow.AddMinutes(OtpRules.ValidityMinutes), row.ExpiresAt);
    }

    [Fact]
    public async Task Verify_RightCode_Succeeds_AndCodeIsSingleUse()
    {
        using var host = await OtpTestHost.CreateAsync();
        using var scope = host.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<OtpService>();
        host.Codes.Enqueue("123456");
        await service.SendAsync(Phone);

        var first = await service.VerifyAsync(Phone, "123456");
        var second = await service.VerifyAsync(Phone, "123456");

        Assert.Equal(OtpVerifyStatus.Verified, first.Status);
        Assert.Equal(OtpVerifyStatus.Expired, second.Status);
    }

    [Fact]
    public async Task Verify_AcceptsPersianAndArabicDigits_AndSurroundingSpaces()
    {
        using var host = await OtpTestHost.CreateAsync();
        using var scope = host.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<OtpService>();
        host.Codes.Enqueue("123456", "123456");

        await service.SendAsync(Phone);
        Assert.Equal(OtpVerifyStatus.Verified, (await service.VerifyAsync(Phone, " ۱۲۳۴۵۶ ")).Status);

        host.Clock.Advance(Cooldown);
        await service.SendAsync(Phone);
        Assert.Equal(OtpVerifyStatus.Verified, (await service.VerifyAsync(Phone, "١٢٣٤٥٦")).Status);
    }

    [Fact]
    public async Task Verify_WrongCode_ReportsAttemptsLeft()
    {
        using var host = await OtpTestHost.CreateAsync();
        using var scope = host.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<OtpService>();
        host.Codes.Enqueue("123456");
        await service.SendAsync(Phone);

        var first = await service.VerifyAsync(Phone, "000000");
        var second = await service.VerifyAsync(Phone, "000001");

        Assert.Equal(OtpVerifyStatus.Invalid, first.Status);
        Assert.Equal(4, first.AttemptsLeft);
        Assert.Equal(3, second.AttemptsLeft);
    }

    [Fact]
    public async Task FiveWrongCodes_LockTheCode_EvenTheRightOneIsRefused()
    {
        using var host = await OtpTestHost.CreateAsync();
        using var scope = host.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<OtpService>();
        host.Codes.Enqueue("123456");
        await service.SendAsync(Phone);

        for (var i = 1; i < OtpRules.MaxAttempts; i++)
            Assert.Equal(OtpVerifyStatus.Invalid, (await service.VerifyAsync(Phone, "000000")).Status);

        var fifth = await service.VerifyAsync(Phone, "000000");
        var afterLock = await service.VerifyAsync(Phone, "123456");

        Assert.Equal(OtpVerifyStatus.TooManyAttempts, fifth.Status);
        Assert.Equal(OtpVerifyStatus.TooManyAttempts, afterLock.Status);
    }

    [Fact]
    public async Task FourWrongCodes_ThenTheRightOne_Succeeds()
    {
        using var host = await OtpTestHost.CreateAsync();
        using var scope = host.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<OtpService>();
        host.Codes.Enqueue("123456");
        await service.SendAsync(Phone);

        for (var i = 0; i < OtpRules.MaxAttempts - 1; i++)
            await service.VerifyAsync(Phone, "000000");

        Assert.Equal(OtpVerifyStatus.Verified, (await service.VerifyAsync(Phone, "123456")).Status);
    }

    [Fact]
    public async Task LockedCode_CanBeReplacedByANewCode()
    {
        using var host = await OtpTestHost.CreateAsync();
        using var scope = host.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<OtpService>();
        host.Codes.Enqueue("111111", "222222");
        await service.SendAsync(Phone);
        for (var i = 0; i < OtpRules.MaxAttempts; i++)
            await service.VerifyAsync(Phone, "000000");

        host.Clock.Advance(Cooldown);
        var resend = await service.SendAsync(Phone);

        Assert.True(resend.Sent);
        Assert.Equal(OtpVerifyStatus.Verified, (await service.VerifyAsync(Phone, "222222")).Status);
    }

    [Fact]
    public async Task Code_IsValidAt119Seconds_AndExpiredAt121Seconds()
    {
        using var host = await OtpTestHost.CreateAsync();
        using var scope = host.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<OtpService>();
        host.Codes.Enqueue("123456", "654321");

        await service.SendAsync(Phone);
        host.Clock.Advance(TimeSpan.FromSeconds(119));
        Assert.Equal(OtpVerifyStatus.Verified, (await service.VerifyAsync(Phone, "123456")).Status);

        host.Clock.Advance(Cooldown);
        await service.SendAsync(Phone);
        host.Clock.Advance(TimeSpan.FromSeconds(121));
        Assert.Equal(OtpVerifyStatus.Expired, (await service.VerifyAsync(Phone, "654321")).Status);
    }

    [Fact]
    public async Task Resend_Before60Seconds_IsTooSoon_AndAfter60SecondsIsAllowed()
    {
        using var host = await OtpTestHost.CreateAsync();
        using var scope = host.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<OtpService>();

        Assert.True((await service.SendAsync(Phone)).Sent);

        host.Clock.Advance(TimeSpan.FromSeconds(20));
        var tooSoon = await service.SendAsync(Phone);
        Assert.Equal(OtpSendDecision.TooSoon, tooSoon.Decision);
        Assert.Equal(40, tooSoon.RetryAfterSeconds);
        Assert.Single(host.Sms.Sent); // پیامک دومی نرفته

        host.Clock.Advance(TimeSpan.FromSeconds(41));
        Assert.True((await service.SendAsync(Phone)).Sent);
        Assert.Equal(2, host.Sms.Sent.Count);
    }

    [Fact]
    public async Task SixthCodeWithinAnHour_IsRejected_UntilTheFirstOneLeavesTheWindow()
    {
        using var host = await OtpTestHost.CreateAsync();
        using var scope = host.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<OtpService>();

        for (var i = 0; i < OtpRules.MaxCodesPerHour; i++)
        {
            if (i > 0) host.Clock.Advance(Cooldown);
            Assert.True((await service.SendAsync(Phone)).Sent);
        }

        host.Clock.Advance(Cooldown);
        var sixth = await service.SendAsync(Phone);
        Assert.Equal(OtpSendDecision.HourlyLimit, sixth.Decision);
        Assert.Equal(OtpRules.MaxCodesPerHour, host.Sms.Sent.Count);

        // یک ساعت و یک ثانیه از کد اول گذشته؛ کد اول از پنجره بیرون رفته
        host.Clock.Advance(TimeSpan.FromMinutes(60) - Cooldown * OtpRules.MaxCodesPerHour + TimeSpan.FromSeconds(2));
        Assert.True((await service.SendAsync(Phone)).Sent);
    }

    [Fact]
    public async Task HourlyLimit_IsPerPhone()
    {
        using var host = await OtpTestHost.CreateAsync();
        using var scope = host.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<OtpService>();

        for (var i = 0; i < OtpRules.MaxCodesPerHour; i++)
        {
            if (i > 0) host.Clock.Advance(Cooldown);
            await service.SendAsync(Phone);
        }

        Assert.True((await service.SendAsync(OtherPhone)).Sent);
    }

    [Fact]
    public async Task NewCode_ReplacesThePreviousOne()
    {
        using var host = await OtpTestHost.CreateAsync();
        using var scope = host.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<OtpService>();
        host.Codes.Enqueue("111111", "222222");

        await service.SendAsync(Phone);
        host.Clock.Advance(Cooldown);
        await service.SendAsync(Phone);

        var old = await service.VerifyAsync(Phone, "111111");
        Assert.Equal(OtpVerifyStatus.Invalid, old.Status);
        Assert.Equal(OtpVerifyStatus.Verified, (await service.VerifyAsync(Phone, "222222")).Status);
    }

    [Fact]
    public async Task Code_OfOnePhone_DoesNotVerifyAnotherPhone()
    {
        using var host = await OtpTestHost.CreateAsync();
        using var scope = host.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<OtpService>();
        host.Codes.Enqueue("123456");
        await service.SendAsync(Phone);

        var other = await service.VerifyAsync(OtherPhone, "123456");

        Assert.Equal(OtpVerifyStatus.Expired, other.Status); // برای آن شماره کدی فعال نیست
        Assert.Equal(OtpVerifyStatus.Verified, (await service.VerifyAsync(Phone, "123456")).Status);
    }

    [Fact]
    public async Task Verify_WithoutAnyCode_IsExpired()
    {
        using var host = await OtpTestHost.CreateAsync();
        using var scope = host.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<OtpService>();

        Assert.Equal(OtpVerifyStatus.Expired, (await service.VerifyAsync(Phone, "123456")).Status);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("12")]
    [InlineData("1234567")]
    [InlineData("12345a")]
    public async Task MalformedCode_IsInvalid_AndDoesNotConsumeAttempts(string? code)
    {
        using var host = await OtpTestHost.CreateAsync();
        using var scope = host.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<OtpService>();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        host.Codes.Enqueue("123456");
        await service.SendAsync(Phone);

        var result = await service.VerifyAsync(Phone, code);

        Assert.Equal(OtpVerifyStatus.Invalid, result.Status);
        Assert.Equal(OtpRules.MaxAttempts, result.AttemptsLeft);
        Assert.Equal(0, (await db.OtpCodes.SingleAsync()).Attempts);
    }

    [Fact]
    public async Task Send_PurgesCodesOlderThanADay()
    {
        using var host = await OtpTestHost.CreateAsync();
        using var scope = host.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<OtpService>();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await service.SendAsync(Phone);
        host.Clock.Advance(TimeSpan.FromHours(25));
        await service.SendAsync(Phone);

        Assert.Equal(1, await db.OtpCodes.CountAsync());
    }

    [Fact]
    public async Task Send_ResultIsTheSame_WhetherOrNotAnAccountExists()
    {
        using var host = await OtpTestHost.CreateAsync();
        using var scope = host.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<OtpService>();
        var registration = scope.ServiceProvider.GetRequiredService<MerajAndisheh.Infrastructure.Identity.PhoneRegistrationService>();
        var adult = await host.GetAgeGroupIdAsync("adult");
        await registration.RegisterAsync(new(Phone, "علی", adult));

        var existing = await service.SendAsync(Phone);
        var unknown = await service.SendAsync(OtherPhone);

        Assert.Equal(existing, unknown);
    }
}
