namespace MerajAndisheh.Core.Domain;

/// <summary>
/// نام Policy های مجوز. برای هر کلید دسترسی (PermissionKeys) یک Policy هم‌نام خودش ثبت می‌شود،
/// مثلاً [Authorize(Policy = PermissionKeys.Users)]؛ این‌ها Policy های بدون کلیدند.
/// </summary>
public static class PolicyNames
{
    /// <summary>فقط مدیر کل (مدیریت همکاران و دسترسی‌ها؛ به کسی واگذار نمی‌شود).</summary>
    public const string SuperAdminOnly = "SuperAdminOnly";

    /// <summary>هر همکار یا مدیر کل (صفحه‌ی اصلی پنل).</summary>
    public const string AnyStaff = "AnyStaff";
}
