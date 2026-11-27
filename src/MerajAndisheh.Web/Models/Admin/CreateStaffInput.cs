namespace MerajAndisheh.Web.Models.Admin;

/// <summary>فرم ساخت حساب همکار.</summary>
public class CreateStaffInput
{
    public string? Email { get; set; }
    public string? DisplayName { get; set; }

    /// <summary>کلیدهای دسترسی تیک‌خورده (PermissionKeys).</summary>
    public List<string> Permissions { get; set; } = [];
}
