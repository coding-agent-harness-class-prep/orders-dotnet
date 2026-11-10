namespace Orders.Domain.Validation;

/// <summary>Stable error codes returned to API clients. Never renumber an existing code.</summary>
public static class ValidationCodes
{
    public const string CustomerRequired = "ORD001";
    public const string UnsupportedRegion = "ORD002";
    public const string NoLines = "ORD003";
    public const string QuantityNotPositive = "ORD004";
    public const string UnknownSku = "ORD005";
}
