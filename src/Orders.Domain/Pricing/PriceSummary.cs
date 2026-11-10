namespace Orders.Domain.Pricing;

public sealed record DiscountBreakdown(decimal BulkDiscount, decimal OrderDiscount, decimal CouponDiscount)
{
    public static readonly DiscountBreakdown None = new(0m, 0m, 0m);

    public decimal Total => BulkDiscount + OrderDiscount + CouponDiscount;
}

public sealed record PriceSummary(decimal Subtotal, DiscountBreakdown Discounts, decimal Tax, decimal Total);
