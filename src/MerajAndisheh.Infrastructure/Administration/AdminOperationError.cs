namespace MerajAndisheh.Infrastructure.Administration;

/// <summary>دلیل ناموفق بودن یک عملیات مدیریتی روی حساب (فعال/غیرفعال، تغییر دسترسی).</summary>
public enum AdminOperationError
{
    /// <summary>کاربر پیدا نشد.</summary>
    NotFound = 1,

    /// <summary>حساب از نوع مورد انتظار نیست (مثلاً همکار در صفحه‌ی کاربران، یا مدیر کل).</summary>
    NotAllowed,

    /// <summary>کلید دسترسی ناشناخته.</summary>
    PermissionInvalid,

    Unknown
}
