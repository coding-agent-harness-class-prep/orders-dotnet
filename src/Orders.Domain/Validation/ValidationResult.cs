namespace Orders.Domain.Validation;

public sealed record ValidationError(string Code, string Message);

public sealed record ValidationResult(IReadOnlyList<ValidationError> Errors)
{
    public static readonly ValidationResult Valid = new([]);

    public bool IsValid => Errors.Count == 0;
}
