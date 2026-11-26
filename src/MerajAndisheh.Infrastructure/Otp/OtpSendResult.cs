using MerajAndisheh.Core.Services;

namespace MerajAndisheh.Infrastructure.Otp;

/// <summary>نتیجه‌ی درخواست ارسال کد. Allowed یعنی کد ساخته و ارسال شد.</summary>
public sealed record OtpSendResult(OtpSendDecision Decision, int RetryAfterSeconds)
{
    public bool Sent => Decision == OtpSendDecision.Allowed;
}
