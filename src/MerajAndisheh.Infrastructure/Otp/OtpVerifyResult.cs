namespace MerajAndisheh.Infrastructure.Otp;

/// <summary>نتیجه‌ی بررسی کد. AttemptsLeft فقط برای Invalid معنی دارد.</summary>
public sealed record OtpVerifyResult(OtpVerifyStatus Status, int AttemptsLeft = 0)
{
    public bool Verified => Status == OtpVerifyStatus.Verified;
}
