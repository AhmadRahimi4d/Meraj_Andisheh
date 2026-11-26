namespace MerajAndisheh.Infrastructure.Otp;

public enum OtpVerifyStatus
{
    /// <summary>کد درست بود و مصرف شد.</summary>
    Verified = 1,

    /// <summary>کد غلط بود (هنوز تلاش باقی است).</summary>
    Invalid,

    /// <summary>کد فعالی نیست یا مهلتش (۲ دقیقه) گذشته.</summary>
    Expired,

    /// <summary>۵ تلاش غلط انجام شده؛ کد قفل است و باید کد تازه گرفت.</summary>
    TooManyAttempts
}
