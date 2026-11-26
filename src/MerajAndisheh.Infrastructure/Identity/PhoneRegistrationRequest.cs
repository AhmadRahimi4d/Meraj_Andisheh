namespace MerajAndisheh.Infrastructure.Identity;

/// <summary>ورودی ثبت‌نام با موبایل (پس از تأیید کد پیامکی).</summary>
public sealed record PhoneRegistrationRequest(
    string? PhoneNumber,
    string? DisplayName,
    int? AgeGroupId);
