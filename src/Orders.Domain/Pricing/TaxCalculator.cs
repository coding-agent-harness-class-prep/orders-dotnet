namespace Orders.Domain.Pricing;

/// <summary>
/// Regional sales tax, charged on the discounted amount (finance policy FIN-7).
/// </summary>
public sealed class TaxCalculator
{
    private static readonly IReadOnlyDictionary<string, decimal> Rates =
        new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
        {
            ["CA"] = 0.0725m,
            ["NY"] = 0.04m,
            ["OR"] = 0m,
            ["TX"] = 0.0625m,
            ["WA"] = 0.065m,
        };

    public bool IsSupportedRegion(string? region) => region is not null && Rates.ContainsKey(region);

    public decimal Calculate(decimal taxableAmount, string region)
    {
        if (!Rates.TryGetValue(region, out var rate))
        {
            throw new ArgumentException($"Unsupported region '{region}'.", nameof(region));
        }

        return Math.Round(taxableAmount * rate, 2);
    }
}
