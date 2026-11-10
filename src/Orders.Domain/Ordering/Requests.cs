using Orders.Domain.Model;
using Orders.Domain.Validation;

namespace Orders.Domain.Ordering;

public sealed record CreateOrderRequest(
    string CustomerId,
    CustomerTier CustomerTier,
    string Region,
    IReadOnlyList<OrderLineRequest> Lines,
    string? CouponCode = null);

public sealed record OrderLineRequest(string Sku, int Quantity);

/// <summary>Either the order that was created, or the validation errors that stopped it.</summary>
public sealed record CreateOrderOutcome(Order? Order, ValidationResult Validation)
{
    public bool Succeeded => Order is not null;
}
