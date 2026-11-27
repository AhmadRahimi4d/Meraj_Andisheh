namespace MerajAndisheh.Core.Domain.Entities;

/// <summary>یک کلید دسترسی که به یک همکار داده شده (بخش ۶.۱؛ کلید ترکیبی UserId + PermissionKey).</summary>
public class StaffPermission
{
    public int UserId { get; set; }

    /// <summary>یکی از PermissionKeys.</summary>
    public string PermissionKey { get; set; } = string.Empty;
}
