using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Orders.Api.Tests;

public class OrderEndpointsTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();

    private static object ValidOrder() => new
    {
        customerId = "C-1001",
        customerTier = "Standard",
        region = "OR",
        lines = new[] { new { sku = "KB-MECH", quantity = 1 } },
    };

    [Fact]
    public async Task GetProducts_ReturnsCatalog()
    {
        var products = await _client.GetFromJsonAsync<JsonElement>("/products", TestContext.Current.CancellationToken);

        Assert.Equal(6, products.GetArrayLength());
    }

    [Fact]
    public async Task PostOrder_Valid_Returns201WithLocation()
    {
        var response = await _client.PostAsJsonAsync("/orders", ValidOrder(), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.StartsWith("/orders/", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task PostOrder_Invalid_Returns400WithErrorCodes()
    {
        var invalid = new { customerId = "", customerTier = "Standard", region = "OR", lines = Array.Empty<object>() };

        var response = await _client.PostAsJsonAsync("/orders", invalid, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.Contains("ORD001", body);
        Assert.Contains("ORD003", body);
    }

    [Fact]
    public async Task GetOrder_Unknown_Returns404()
    {
        var response = await _client.GetAsync($"/orders/{Guid.NewGuid()}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
