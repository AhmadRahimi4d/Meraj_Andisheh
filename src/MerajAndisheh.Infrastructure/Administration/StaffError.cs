namespace MerajAndisheh.Infrastructure.Administration;

/// <summary>دلیل ناموفق بودن ساخت حساب همکار. فقط کد است؛ متن فارسی از resx می‌آید.</summary>
public enum StaffError
{
    EmailInvalid = 1,
    EmailTaken,
    DisplayNameInvalid,
    PermissionInvalid,
    Unknown
}
