using Orders.Domain.Model;

namespace Orders.Domain.Pricing;

/// <summary>
/// The commercial discount rules agreed with Sales. Bulk discounts are applied per line first;
/// the order-level discount and the welcome coupon are worked out on what is left.
/// </summary>
public sealed class DiscountPolicy
{
    private const int BulkQuantity = 50;
    private const decimal BulkRate = 0.08m;
    private const decimal OrderDiscountCap = 0.12m;
    private const string WelcomeCoupon = "WELCOME10";
    private const decimal WelcomeCouponAmount = 10m;

    // Highest threshold first: the first tier the amount reaches wins.
    private static readonly (decimal Threshold, decimal Rate)[] VolumeTiers =
    [
        (5_000m, 0.10m),
        (1_000m, 0.05m),
    ];

    private static readonly IReadOnlyDictionary<CustomerTier, decimal> LoyaltyBonus =
        new Dictionary<CustomerTier, decimal>
        {
            [CustomerTier.Standard] = 0m,
            [CustomerTier.Silver] = 0.02m,
            [CustomerTier.Gold] = 0.04m,
        };

    public DiscountBreakdown Calculate(IReadOnlyList<OrderLine> lines, CustomerTier tier, string? couponCode)
    {
        if (lines.Count == 0)
        {
            return DiscountBreakdown.None;
        }

        var bulk = lines
            .Where(l => l.Quantity >= BulkQuantity)
            .Sum(l => Round(l.LineTotal * BulkRate));

        var afterBulk = lines.Sum(l => l.LineTotal) - bulk;

        var rate = VolumeTiers.FirstOrDefault(t => afterBulk >= t.Threshold).Rate + LoyaltyBonus[tier];
        var orderDiscount = Round(afterBulk * Math.Min(rate, OrderDiscountCap));

        var coupon = 0m;
        if (orderDiscount == 0m && string.Equals(couponCode, WelcomeCoupon, StringComparison.OrdinalIgnoreCase))
        {
            coupon = Math.Min(WelcomeCouponAmount, afterBulk);
        }

        return new DiscountBreakdown(bulk, orderDiscount, coupon);
    }

    private static decimal Round(decimal amount) => Math.Round(amount, 2, MidpointRounding.AwayFromZero);
}
