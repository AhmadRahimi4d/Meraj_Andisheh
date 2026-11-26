namespace MerajAndisheh.Core.Services;

/// <summary>
/// مقصد بعد از ورود (بخش ۳.۱۴): عضو ← صفحه‌ی قبلی یا صفحه‌ی اصلی؛ همکار و مدیر کل ← پنل مدیریت.
/// </summary>
public static class LoginRedirectRules
{
    /// <param name="roles">نقش‌های کاربر.</param>
    /// <param name="safeReturnUrl">نشانی بازگشت که پیش‌تر محلی‌بودنش بررسی شده؛ در غیر این صورت null.</param>
    public static string ResolveTarget(IEnumerable<string> roles, string? safeReturnUrl)
    {
        if (AdminAccessRules.HasStaffRole(roles)) return AdminAccessRules.AdminPath;

        return string.IsNullOrEmpty(safeReturnUrl) ? "/" : safeReturnUrl;
    }
}
