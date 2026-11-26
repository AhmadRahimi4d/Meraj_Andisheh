using MerajAndisheh.Core.Domain;
using MerajAndisheh.Core.Services;
using Xunit;

namespace MerajAndisheh.Tests;

/// <summary>قاعده‌ی ارسال دوباره (بخش ۳.۱۴): ۶۰ ثانیه فاصله و حداکثر ۵ کد در ساعت.</summary>
public class OtpSendGateTests
{
    private static readonly DateTime Now = new(2026, 11, 26, 10, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Rules_MatchDesignDoc()
    {
        Assert.Equal(6, OtpRules.CodeLength);
        Assert.Equal(2, OtpRules.ValidityMinutes);
        Assert.Equal(5, OtpRules.MaxAttempts);
        Assert.Equal(60, OtpRules.ResendCooldownSeconds);
        Assert.Equal(5, OtpRules.MaxCodesPerHour);
    }

    [Fact]
    public void FirstCode_IsAllowed()
    {
        var result = OtpSendGate.Evaluate(Now, null, 0);

        Assert.Equal(OtpSendDecision.Allowed, result.Decision);
    }

    [Theory]
    [InlineData(0, 60)]
    [InlineData(10, 50)]
    [InlineData(59, 1)]
    public void ResendBefore60Seconds_IsTooSoon_WithRemainingSeconds(int elapsedSeconds, int expectedWait)
    {
        var result = OtpSendGate.Evaluate(Now, Now.AddSeconds(-elapsedSeconds), 1);

        Assert.Equal(OtpSendDecision.TooSoon, result.Decision);
        Assert.Equal(expectedWait, result.RetryAfterSeconds);
    }

    [Fact]
    public void ResendAfterExactly60Seconds_IsAllowed()
    {
        var result = OtpSendGate.Evaluate(Now, Now.AddSeconds(-60), 1);

        Assert.Equal(OtpSendDecision.Allowed, result.Decision);
    }

    [Fact]
    public void FifthCodeInAnHour_IsAllowed_SixthIsRejected()
    {
        var last = Now.AddSeconds(-120);

        Assert.Equal(OtpSendDecision.Allowed, OtpSendGate.Evaluate(Now, last, 4).Decision);
        Assert.Equal(OtpSendDecision.HourlyLimit, OtpSendGate.Evaluate(Now, last, 5).Decision);
        Assert.Equal(OtpSendDecision.HourlyLimit, OtpSendGate.Evaluate(Now, last, 9).Decision);
    }

    [Fact]
    public void Cooldown_IsCheckedBeforeHourlyLimit()
    {
        var result = OtpSendGate.Evaluate(Now, Now.AddSeconds(-5), 5);

        Assert.Equal(OtpSendDecision.TooSoon, result.Decision);
    }
}
