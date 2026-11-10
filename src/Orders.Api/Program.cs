using System.Text.Json.Serialization;
using Orders.Domain.Catalog;
using Orders.Domain.Model;
using Orders.Domain.Ordering;
using Orders.Domain.Pricing;
using Orders.Domain.Validation;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddProblemDetails();

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<IProductCatalog>(_ => new InMemoryProductCatalog());
builder.Services.AddSingleton<IOrderRepository, InMemoryOrderRepository>();
builder.Services.AddSingleton<DiscountPolicy>();
builder.Services.AddSingleton<TaxCalculator>();
builder.Services.AddSingleton<OrderValidator>();
builder.Services.AddSingleton<OrderService>();

var app = builder.Build();

app.MapGet("/products", (IProductCatalog catalog) => catalog.All());

app.MapGet("/orders", (OrderService service, OrderStatus? status) => service.List(status));

app.MapGet("/orders/{id:guid}", (Guid id, OrderService service) =>
    service.Find(id) is { } order ? Results.Ok(order) : Results.NotFound());

app.MapPost("/orders", (CreateOrderRequest request, OrderService service) =>
{
    var outcome = service.Create(request);
    if (outcome.Order is { } order)
    {
        return Results.Created($"/orders/{order.Id}", order);
    }

    return Results.Problem(
        statusCode: StatusCodes.Status400BadRequest,
        title: "The order is not valid.",
        extensions: new Dictionary<string, object?> { ["errors"] = outcome.Validation.Errors });
});

app.Run();

// Exposes the entry point to WebApplicationFactory in the API tests.
public partial class Program;
