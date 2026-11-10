using Orders.Domain.Catalog;
using Orders.Domain.Model;
using Orders.Domain.Pricing;
using Orders.Domain.Validation;

namespace Orders.Domain.Ordering;

public sealed class OrderService(
    IOrderRepository orders,
    IProductCatalog catalog,
    OrderValidator validator,
    DiscountPolicy discountPolicy,
    TaxCalculator taxCalculator,
    TimeProvider clock)
{
    public CreateOrderOutcome Create(CreateOrderRequest request)
    {
        var validation = validator.Validate(request);
        if (!validation.IsValid)
        {
            return new CreateOrderOutcome(null, validation);
        }

        var lines = request.Lines
            .Select(l => new OrderLine(l.Sku, l.Quantity, catalog.Find(l.Sku)!.UnitPrice))
            .ToList();

        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = request.CustomerId,
            CustomerTier = request.CustomerTier,
            Region = request.Region.ToUpperInvariant(),
            Lines = lines,
            CouponCode = request.CouponCode,
            Price = Price(lines, request.CustomerTier, request.Region, request.CouponCode),
            CreatedAt = clock.GetUtcNow(),
        };

        orders.Save(order);
        return new CreateOrderOutcome(order, validation);
    }

    public Order? Find(Guid id) => orders.Find(id);

    public IReadOnlyList<Order> List(OrderStatus? status = null) =>
        [.. orders.All().Where(o => status is null || o.Status == status)];

    private PriceSummary Price(IReadOnlyList<OrderLine> lines, CustomerTier tier, string region, string? couponCode)
    {
        var subtotal = lines.Sum(l => l.LineTotal);
        var discounts = discountPolicy.Calculate(lines, tier, couponCode);
        var taxable = subtotal - discounts.Total;
        var tax = taxCalculator.Calculate(taxable, region);
        return new PriceSummary(subtotal, discounts, tax, taxable + tax);
    }
}
