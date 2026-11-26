namespace MerajAndisheh.Infrastructure.Identity;

/// <summary>نتیجه‌ی ثبت‌نام با موبایل.</summary>
public sealed class PhoneRegistrationResult
{
    private PhoneRegistrationResult(AppUser? user, IReadOnlyList<RegistrationError> errors)
    {
        User = user;
        Errors = errors;
    }

    public bool Succeeded => User is not null && Errors.Count == 0;

    public AppUser? User { get; }
    public IReadOnlyList<RegistrationError> Errors { get; }

    public static PhoneRegistrationResult Success(AppUser user) => new(user, []);

    public static PhoneRegistrationResult Failure(IReadOnlyList<RegistrationError> errors) => new(null, errors);
}
