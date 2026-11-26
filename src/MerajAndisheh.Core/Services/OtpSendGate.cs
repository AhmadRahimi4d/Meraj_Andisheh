using MerajAndisheh.Core.Domain;

namespace MerajAndisheh.Core.Services;

/// <summary>
/// قاعده‌ی ارسال دوباره (بخش ۳.۱۴): حداقل ۶۰ ثانیه بین دو ارسال و حداکثر ۵ کد در ساعت برای هر شماره.
/// منطق خالص و بدون دیتابیس است تا مستقیم تست شود.
/// </summary>
public static class OtpSendGate
{
    public static OtpSendEvaluation Evaluate(DateTime nowUtc, DateTime? lastSentAtUtc, int codesSentInLastHour)
    {
        if (lastSentAtUtc is { } last)
        {
            var elapsedSeconds = (nowUtc - last).TotalSeconds;
            if (elapsedSeconds < OtpRules.ResendCooldownSeconds)
            {
                var wait = (int)Math.Ceiling(OtpRules.ResendCooldownSeconds - elapsedSeconds);
                return new OtpSendEvaluation(OtpSendDecision.TooSoon, wait);
            }
        }

        if (codesSentInLastHour >= OtpRules.MaxCodesPerHour)
            return new OtpSendEvaluation(OtpSendDecision.HourlyLimit, 0);

        return new OtpSendEvaluation(OtpSendDecision.Allowed, 0);
    }
}
