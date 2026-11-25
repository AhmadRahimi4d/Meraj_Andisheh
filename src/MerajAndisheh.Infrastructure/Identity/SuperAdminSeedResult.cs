namespace MerajAndisheh.Infrastructure.Identity;

public enum SuperAdminSeedResult
{
    /// <summary>مدیر کل ساخته شد.</summary>
    Created = 1,

    /// <summary>از قبل یک مدیر کل وجود دارد؛ کاری انجام نشد.</summary>
    AlreadyExists,

    /// <summary>ایمیل یا رمز موقت در تنظیمات مخفی نیست؛ کاری انجام نشد.</summary>
    NotConfigured,

    /// <summary>ساخت ناموفق بود (مثلاً رمز کوتاه یا ایمیل تکراری).</summary>
    Failed
}
