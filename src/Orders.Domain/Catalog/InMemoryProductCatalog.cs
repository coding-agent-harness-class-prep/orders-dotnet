using Orders.Domain.Model;

namespace Orders.Domain.Catalog;

public interface IProductCatalog
{
    IReadOnlyList<Product> All();

    Product? Find(string sku);
}

public sealed class InMemoryProductCatalog : IProductCatalog
{
    private readonly Dictionary<string, Product> _products;

    public InMemoryProductCatalog()
        : this(
        [
            new("CAB-USBC", "USB-C cable, 1 m", 8.40m),
            new("DOCK-TB4", "Thunderbolt 4 dock", 199.00m),
            new("HS-NC", "Noise-canceling headset", 129.99m),
            new("KB-MECH", "Mechanical keyboard", 89.00m),
            new("MON-27", "27-inch monitor", 249.00m),
            new("MS-WL", "Wireless mouse", 24.50m),
        ])
    {
    }

    public InMemoryProductCatalog(IEnumerable<Product> products) =>
        _products = products.ToDictionary(p => p.Sku, StringComparer.OrdinalIgnoreCase);

    public IReadOnlyList<Product> All() => [.. _products.Values.OrderBy(p => p.Sku)];

    public Product? Find(string sku) => _products.GetValueOrDefault(sku);
}
