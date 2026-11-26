namespace MerajAndisheh.Core.Services;

/// <summary>نتیجه‌ی بررسی اجازه‌ی ارسال کد تازه برای یک شماره.</summary>
public enum OtpSendDecision
{
    /// <summary>ارسال مجاز است.</summary>
    Allowed = 1,

    /// <summary>کمتر از ۶۰ ثانیه از ارسال قبلی گذشته است.</summary>
    TooSoon,

    /// <summary>در یک ساعت اخیر به سقف ۵ کد رسیده است.</summary>
    HourlyLimit
}
