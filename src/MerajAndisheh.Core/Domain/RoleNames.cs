namespace MerajAndisheh.Core.Domain;

/// <summary>نقش‌های سیستم (بخش ۲ سند). دسترسی همکاران بخش‌به‌بخش و با PermissionKey است (مرحله‌ی ۶).</summary>
public static class RoleNames
{
    public const string SuperAdmin = "SuperAdmin";
    public const string Staff = "Staff";
    public const string Member = "Member";

    public static IReadOnlyList<string> All { get; } = [SuperAdmin, Staff, Member];
}
