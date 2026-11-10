using Orders.Domain.Catalog;
using Orders.Domain.Model;
using Orders.Domain.Ordering;
using Orders.Domain.Pricing;
using Orders.Domain.Validation;

namespace Orders.Domain.Tests;

public class OrderServiceTests
{
    private readonly InMemoryOrderRepository _repository = new();
    private readonly OrderService _service;

    public OrderServiceTests()
    {
        var catalog = new InMemoryProductCatalog();
        var tax = new TaxCalculator();
        _service = new OrderService(
            _repository,
            catalog,
            new OrderValidator(catalog, tax),
            new DiscountPolicy(),
            tax,
            TimeProvider.System);
    }

    [Fact]
    public void Create_ValidRequest_SavesPendingOrderWithCatalogPrices()
    {
        // Oregon has no sales tax and the order is too small for any discount.
        var request = new CreateOrderRequest("C-1001", CustomerTier.Standard, "OR",
            [new OrderLineRequest("KB-MECH", 2), new OrderLineRequest("MS-WL", 1)]);

        var outcome = _service.Create(request);

        Assert.True(outcome.Succeeded);
        var order = outcome.Order!;
        Assert.Equal(OrderStatus.Pending, order.Status);
        Assert.Equal(89.00m, order.Lines[0].UnitPrice);
        Assert.Equal(202.50m, order.Price.Subtotal);
        Assert.Equal(202.50m, order.Price.Total);
        Assert.Same(order, _repository.Find(order.Id));
    }

    [Fact]
    public void Create_InvalidRequest_ReturnsErrorsAndSavesNothing()
    {
        var request = new CreateOrderRequest("", CustomerTier.Standard, "OR", []);

        var outcome = _service.Create(request);

        Assert.False(outcome.Succeeded);
        Assert.Contains(outcome.Validation.Errors, e => e.Code == ValidationCodes.CustomerRequired);
        Assert.Empty(_repository.All());
    }

    [Fact]
    public void List_WithStatus_ReturnsOnlyMatchingOrders()
    {
        var first = _service.Create(new("C-1", CustomerTier.Standard, "OR", [new("MS-WL", 1)])).Order!;
        _service.Create(new("C-2", CustomerTier.Standard, "OR", [new("MS-WL", 1)]));
        first.Status = OrderStatus.Shipped;

        var shipped = _service.List(OrderStatus.Shipped);

        Assert.Equal([first.Id], shipped.Select(o => o.Id));
    }
}
