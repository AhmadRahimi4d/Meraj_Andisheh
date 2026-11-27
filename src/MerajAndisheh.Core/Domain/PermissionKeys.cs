namespace MerajAndisheh.Core.Domain;

/// <summary>
/// کلیدهای دسترسی همکاران، بخش‌به‌بخش (بخش ۲ سند). مدیر کل همه را ضمنی دارد.
/// «مدیریت همکاران» کلید ندارد و فقط برای مدیر کل است (PolicyNames.SuperAdminOnly).
/// </summary>
public static class PermissionKeys
{
    public const string Dashboard = "dashboard.view";
    public const string Content = "content.manage";
    public const string Taxonomy = "taxonomy.manage";
    public const string Requests = "requests.manage";
    public const string Comments = "comments.moderate";
    public const string Users = "users.manage";
    public const string Layout = "layout.manage";
    public const string Portals = "portals.manage";
    public const string Settings = "settings.manage";
    public const string Backup = "backup.manage";

    /// <summary>همه‌ی کلیدها به ترتیب نمایش در فرم‌ها.</summary>
    public static IReadOnlyList<string> All { get; } =
        [Dashboard, Content, Taxonomy, Requests, Comments, Users, Layout, Portals, Settings, Backup];
}
