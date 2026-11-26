using MerajAndisheh.Core.Domain;

namespace MerajAndisheh.Core.Services;

/// <summary>
/// قاعده‌ی دسترسی به پنل مدیریت (بخش ۳.۱۴): هیچ لینکی به پنل در بخش عمومی نیست و هر کسی جز
/// همکار و مدیر کل (عضو یا ورودنکرده) با نشانی مستقیم، «یافت نشد (۴۰۴)» می‌گیرد.
/// </summary>
public static class AdminAccessRules
{
    public const string AdminPath = "/admin";

    /// <summary>آیا مسیر درخواست، خود پنل یا زیرمسیر آن است؟ (بدون حساسیت به بزرگی و کوچکی حروف)</summary>
    public static bool IsAdminPath(string? path)
    {
        if (string.IsNullOrEmpty(path)) return false;

        return path.Equals(AdminPath, StringComparison.OrdinalIgnoreCase)
            || path.StartsWith(AdminPath + "/", StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsStaffRole(string? role) => role is RoleNames.SuperAdmin or RoleNames.Staff;

    public static bool HasStaffRole(IEnumerable<string> roles) => roles.Any(IsStaffRole);
}
