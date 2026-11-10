using Orders.Domain.Catalog;
using Orders.Domain.Ordering;
using Orders.Domain.Pricing;

namespace Orders.Domain.Validation;

public sealed class OrderValidator(IProductCatalog catalog, TaxCalculator taxCalculator)
{
    public ValidationResult Validate(CreateOrderRequest request)
    {
        var errors = new List<ValidationError>();

        if (string.IsNullOrWhiteSpace(request.CustomerId))
        {
            errors.Add(new(ValidationCodes.CustomerRequired, "A customer ID is required."));
        }

        if (!taxCalculator.IsSupportedRegion(request.Region))
        {
            errors.Add(new(ValidationCodes.UnsupportedRegion, $"We don't ship to region '{request.Region}'."));
        }

        if (request.Lines.Count == 0)
        {
            errors.Add(new(ValidationCodes.NoLines, "An order needs at least one line."));
        }

        foreach (var line in request.Lines)
        {
            if (line.Quantity <= 0)
            {
                errors.Add(new(ValidationCodes.QuantityNotPositive, $"The quantity for {line.Sku} must be at least 1."));
            }

            if (catalog.Find(line.Sku) is null)
            {
                errors.Add(new(ValidationCodes.UnknownSku, $"{line.Sku} is not in the catalog."));
            }
        }

        return errors.Count == 0 ? ValidationResult.Valid : new ValidationResult(errors);
    }
}
