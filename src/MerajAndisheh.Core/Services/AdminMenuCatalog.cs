using MerajAndisheh.Core.Domain;

namespace MerajAndisheh.Core.Services;

/// <summary>
/// فهرست بخش‌های پنل و قاعده‌ی دیده‌شدن آن‌ها در منو (بخش ۸.۲ سند): همکار فقط بخش‌هایی را می‌بیند که دسترسی دارد.
/// بخش‌های هر مرحله هنگام ساخته‌شدنِ همان مرحله به این فهرست اضافه می‌شود.
/// </summary>
public static class AdminMenuCatalog
{
    public static IReadOnlyList<AdminMenuItem> Items { get; } =
    [
        new("home", "Panel.Menu.Home", AdminAccessRules.AdminPath, null, false),
        new("staff", "Panel.Menu.Staff", AdminAccessRules.AdminPath + "/staff", null, true),
        new("users", "Panel.Menu.Users", AdminAccessRules.AdminPath + "/users", PermissionKeys.Users, false),
        new("settings", "Panel.Menu.Settings", AdminAccessRules.AdminPath + "/settings", PermissionKeys.Settings, false),
    ];

    public static IReadOnlyList<AdminMenuItem> VisibleFor(bool isSuperAdmin, IEnumerable<string> grantedKeys)
    {
        var granted = grantedKeys.ToList();

        return Items.Where(item =>
        {
            if (item.SuperAdminOnly) return isSuperAdmin;
            if (item.Permission is null) return true;
            return PermissionRules.HasPermission(isSuperAdmin, isStaff: true, granted, item.Permission);
        }).ToList();
    }
}
