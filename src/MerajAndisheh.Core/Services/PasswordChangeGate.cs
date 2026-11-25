namespace MerajAndisheh.Core.Services;

/// <summary>
/// قاعده‌ی «تغییر رمز اجباری» (بخش ۳.۱۵): کاربری که باید رمز موقتش را عوض کند، تا آن زمان
/// به هیچ صفحه‌ای جز صفحه‌ی تغییر رمز، خروج و فایل‌های ثابت نمی‌رود.
/// </summary>
public static class PasswordChangeGate
{
    public const string ChangePasswordPath = "/account/change-password";

    private static readonly string[] AllowedPrefixes =
    [
        "/account/change-password",
        "/account/logout",
        "/health",
        "/css/", "/js/", "/fonts/", "/lib/", "/favicon",
        "/_framework", "/_content"
    ];

    public static bool ShouldRedirect(bool isAuthenticated, bool mustChangePassword, string? path)
    {
        if (!isAuthenticated || !mustChangePassword) return false;

        var normalized = string.IsNullOrEmpty(path) ? "/" : path.ToLowerInvariant();
        return !AllowedPrefixes.Any(prefix => normalized.StartsWith(prefix, StringComparison.Ordinal));
    }
}
