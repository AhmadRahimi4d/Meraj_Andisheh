namespace MerajAndisheh.Infrastructure.Administration;

/// <summary>ورودی ساخت حساب همکار توسط مدیر کل.</summary>
public sealed record CreateStaffRequest(string? Email, string? DisplayName, IReadOnlyCollection<string>? Permissions);
