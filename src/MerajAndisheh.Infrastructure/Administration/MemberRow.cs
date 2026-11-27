namespace MerajAndisheh.Infrastructure.Administration;

/// <summary>یک عضو در فهرست کاربران.</summary>
public sealed record MemberRow(
    int Id,
    string DisplayName,
    string? Email,
    string? PhoneNumber,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? LastLoginAt);
