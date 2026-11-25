namespace MerajAndisheh.Infrastructure.Identity;

/// <summary>ورودی ثبت‌نام با ایمیل.</summary>
public sealed record RegistrationRequest(
    string? Email,
    string? Password,
    string? DisplayName,
    int? AgeGroupId);
