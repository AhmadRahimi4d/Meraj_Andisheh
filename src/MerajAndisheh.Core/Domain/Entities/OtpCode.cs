namespace MerajAndisheh.Core.Domain.Entities;

/// <summary>
/// یک کد پیامکی صادرشده (بخش ۶.۱). خود کد ذخیره نمی‌شود؛ فقط هش آن (HMAC با کلید محرمانه‌ی سرور).
/// فقط جدیدترین کد استفاده‌نشده‌ی هر شماره معتبر است.
/// </summary>
public class OtpCode
{
    public int Id { get; set; }

    /// <summary>شماره‌ی یکسان‌سازی‌شده با قالب 09xxxxxxxxx.</summary>
    public string PhoneNumber { get; set; } = string.Empty;

    public string CodeHash { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }

    /// <summary>تعداد تلاش‌های غلط برای همین کد.</summary>
    public int Attempts { get; set; }

    /// <summary>زمان مصرف‌شدن یا جایگزین‌شدن با کد تازه؛ null یعنی هنوز قابل‌استفاده است.</summary>
    public DateTime? UsedAt { get; set; }

    public DateTime CreatedAt { get; set; }
}
