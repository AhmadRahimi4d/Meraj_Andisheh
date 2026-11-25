namespace MerajAndisheh.Infrastructure.Identity;

/// <summary>نتیجه‌ی ثبت‌نام. در صورت موفقیت، توکن تأیید ایمیل (خام، بدون کدگذاری URL) هم برمی‌گردد.</summary>
public sealed class RegistrationResult
{
    private RegistrationResult(AppUser? user, string? confirmationToken, IReadOnlyList<RegistrationError> errors)
    {
        User = user;
        ConfirmationToken = confirmationToken;
        Errors = errors;
    }

    public bool Succeeded => User is not null && Errors.Count == 0;

    public AppUser? User { get; }
    public string? ConfirmationToken { get; }
    public IReadOnlyList<RegistrationError> Errors { get; }

    public static RegistrationResult Success(AppUser user, string confirmationToken) => new(user, confirmationToken, []);

    public static RegistrationResult Failure(IReadOnlyList<RegistrationError> errors) => new(null, null, errors);
}
