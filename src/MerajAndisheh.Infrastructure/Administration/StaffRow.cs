namespace MerajAndisheh.Infrastructure.Administration;

/// <summary>یک همکار در فهرست و صفحه‌ی ویرایش.</summary>
public sealed record StaffRow(
    int Id,
    string DisplayName,
    string? Email,
    bool IsActive,
    bool MustChangePassword,
    DateTime CreatedAt,
    DateTime? LastLoginAt,
    IReadOnlyList<string> Permissions);
