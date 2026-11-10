using Orders.Domain.Catalog;
using Orders.Domain.Model;
using Orders.Domain.Ordering;
using Orders.Domain.Pricing;
using Orders.Domain.Validation;

namespace Orders.Domain.Tests;

public class OrderValidatorTests
{
    private readonly OrderValidator _validator = new(new InMemoryProductCatalog(), new TaxCalculator());

    private static CreateOrderRequest ValidRequest() =>
        new("C-1001", CustomerTier.Standard, "WA", [new OrderLineRequest("KB-MECH", 1)]);

    [Fact]
    public void Validate_ValidRequest_ReturnsNoErrors()
    {
        var result = _validator.Validate(ValidRequest());

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_MissingCustomer_ReturnsCustomerRequired(string customerId)
    {
        var result = _validator.Validate(ValidRequest() with { CustomerId = customerId });

        Assert.Contains(result.Errors, e => e.Code == ValidationCodes.CustomerRequired);
    }

    [Fact]
    public void Validate_UnsupportedRegion_ReturnsUnsupportedRegion()
    {
        var result = _validator.Validate(ValidRequest() with { Region = "ZZ" });

        Assert.Contains(result.Errors, e => e.Code == ValidationCodes.UnsupportedRegion);
    }

    [Fact]
    public void Validate_NoLines_ReturnsNoLines()
    {
        var result = _validator.Validate(ValidRequest() with { Lines = [] });

        Assert.Contains(result.Errors, e => e.Code == ValidationCodes.NoLines);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-3)]
    public void Validate_QuantityNotPositive_ReturnsQuantityNotPositive(int quantity)
    {
        var result = _validator.Validate(ValidRequest() with { Lines = [new OrderLineRequest("KB-MECH", quantity)] });

        var error = Assert.Single(result.Errors);
        Assert.Equal(ValidationCodes.QuantityNotPositive, error.Code);
    }

    [Fact]
    public void Validate_UnknownSku_ReturnsUnknownSku()
    {
        var result = _validator.Validate(ValidRequest() with { Lines = [new OrderLineRequest("NOPE-1", 1)] });

        var error = Assert.Single(result.Errors);
        Assert.Equal(ValidationCodes.UnknownSku, error.Code);
        Assert.Contains("NOPE-1", error.Message);
    }
}
