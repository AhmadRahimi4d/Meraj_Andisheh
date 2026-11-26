namespace MerajAndisheh.Core.Services;

/// <summary>تصمیم ارسال + (برای TooSoon) چند ثانیه‌ی دیگر باید صبر کرد.</summary>
public readonly record struct OtpSendEvaluation(OtpSendDecision Decision, int RetryAfterSeconds);
