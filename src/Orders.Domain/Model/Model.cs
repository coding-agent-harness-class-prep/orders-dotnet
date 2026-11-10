using Orders.Domain.Pricing;

namespace Orders.Domain.Model;

/// <summary>Loyalty tier of the customer placing the order.</summary>
public enum CustomerTier
{
    Standard,
    Silver,
    Gold,
}

public enum OrderStatus
{
    Pending,
    Paid,
    Shipped,
    Canceled,
}

public sealed record Product(string Sku, string Name, decimal UnitPrice);

/// <summary>One line of an order. The unit price is captured from the catalog when the order is placed.</summary>
public sealed record OrderLine(string Sku, int Quantity, decimal UnitPrice)
{
    public decimal LineTotal => UnitPrice * Quantity;
}

public sealed class Order
{
    public required Guid Id { get; init; }

    public required string CustomerId { get; init; }

    public required CustomerTier CustomerTier { get; init; }

    /// <summary>Two-letter shipping region, used to look up the sales-tax rate.</summary>
    public required string Region { get; init; }

    public required IReadOnlyList<OrderLine> Lines { get; init; }

    public string? CouponCode { get; init; }

    public required PriceSummary Price { get; init; }

    public required DateTimeOffset CreatedAt { get; init; }

    public OrderStatus Status { get; set; } = OrderStatus.Pending;
}
