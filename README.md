# Orders service (.NET)

A small order-taking API: a product catalog, order validation, discounts, and regional sales tax.
It's an ASP.NET Core minimal API on .NET 10 with an in-memory store, so there's no database to set up.

## Layout

```
Orders.slnx
src/
  Orders.Domain/         # the business rules — no web dependencies
    Model/               # Order, OrderLine, Product, CustomerTier, OrderStatus
    Pricing/             # DiscountPolicy, TaxCalculator, PriceSummary
    Validation/          # OrderValidator and its error codes
    Catalog/             # the product catalog
    Ordering/            # OrderService and the order repository
  Orders.Api/            # HTTP endpoints (Program.cs) and a sample .http file
tests/
  Orders.Domain.Tests/   # xUnit tests for the domain
  Orders.Api.Tests/      # endpoint tests through WebApplicationFactory
```

## Build, test, run

```powershell
dotnet build
dotnet test
dotnet run --project src/Orders.Api      # http://localhost:5080
```

`global.json` opts `dotnet test` into Microsoft.Testing.Platform, which xUnit v3 uses. Package versions
are pinned in `Directory.Packages.props` and each project commits its `packages.lock.json`.

## Endpoints

| Method | Path | What it does |
|---|---|---|
| `GET` | `/products` | The catalog |
| `GET` | `/orders?status=Pending` | Orders, optionally filtered by status |
| `GET` | `/orders/{id}` | One order, or 404 |
| `POST` | `/orders` | Place an order: 201 with the priced order, or 400 with error codes |

A request to place an order:

```json
{
  "customerId": "C-1001",
  "customerTier": "Silver",
  "region": "WA",
  "lines": [ { "sku": "MON-27", "quantity": 2 } ],
  "couponCode": null
}
```

## Pricing in one paragraph

The subtotal is the sum of the lines at catalog prices. `DiscountPolicy` takes off bulk, volume, loyalty,
and coupon discounts. `TaxCalculator` then charges the shipping region's sales tax on what's left
(finance policy FIN-7). Both work in `decimal` to the cent.
