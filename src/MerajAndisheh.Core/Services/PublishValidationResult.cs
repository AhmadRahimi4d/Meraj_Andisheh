namespace MerajAndisheh.Core.Services;

/// <summary>نتیجه‌ی اعتبارسنجی انتشار: فهرست کدهای خطا (خالی یعنی قابل انتشار).</summary>
public sealed class PublishValidationResult
{
    public PublishValidationResult(IReadOnlyList<PublishError> errors)
    {
        Errors = errors;
    }

    public IReadOnlyList<PublishError> Errors { get; }

    public bool IsValid => Errors.Count == 0;

    public bool Has(PublishError error) => Errors.Contains(error);
}
