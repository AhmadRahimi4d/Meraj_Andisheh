namespace MerajAndisheh.Infrastructure.Administration;

/// <summary>
/// نتیجه‌ی ساخت همکار. TemporaryPassword فقط در همین نتیجه هست و جای دیگری ذخیره نمی‌شود
/// (در دیتابیس فقط هش رمز می‌ماند)؛ صفحه آن را یک‌بار به مدیر کل نشان می‌دهد.
/// </summary>
public sealed class StaffCreateResult
{
    private StaffCreateResult(int? userId, string? temporaryPassword, IReadOnlyList<StaffError> errors)
    {
        UserId = userId;
        TemporaryPassword = temporaryPassword;
        Errors = errors;
    }

    public bool Succeeded => UserId is not null && Errors.Count == 0;

    public int? UserId { get; }
    public string? TemporaryPassword { get; }
    public IReadOnlyList<StaffError> Errors { get; }

    public static StaffCreateResult Success(int userId, string temporaryPassword) => new(userId, temporaryPassword, []);

    public static StaffCreateResult Failure(IReadOnlyList<StaffError> errors) => new(null, null, errors);
}
