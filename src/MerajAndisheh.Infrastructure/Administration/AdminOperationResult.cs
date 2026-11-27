namespace MerajAndisheh.Infrastructure.Administration;

public sealed record AdminOperationResult(AdminOperationError? Error)
{
    public bool Succeeded => Error is null;

    public static AdminOperationResult Ok { get; } = new((AdminOperationError?)null);

    public static AdminOperationResult Fail(AdminOperationError error) => new(error);
}
