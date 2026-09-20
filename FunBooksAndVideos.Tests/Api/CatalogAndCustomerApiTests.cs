using System.Net;
using System.Text.Json;
using FunBooksAndVideos.Api.Contracts.Customers;
using FunBooksAndVideos.Api.Contracts.Products;
using FunBooksAndVideos.Application.Products;
using FunBooksAndVideos.Domain.Products;
using FunBooksAndVideos.Tests.Infrastructure;

namespace FunBooksAndVideos.Tests.Api;

[Collection(SqlServerCollection.Name)]
public class CatalogAndCustomerApiTests(SqlServerFixture fixture)
{
    [SqlServerFact]
    public async Task The_catalog_lists_every_product_with_its_kind_and_categories()
    {
        await using var factory = new ApiFactory(fixture.NewConnectionString());
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/v1/products");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var products = await ApiFactory.ReadAsync<List<ProductResponse>>(response);
        Assert.Equal(7, products.Count);
        var book = products[1];
        Assert.Equal((2L, "The Girl on the train", 9.50m, ProductKind.Physical), (book.Id, book.Name, book.Price, book.Kind));
        Assert.Equal(new[] { ProductCategory.Book }, book.Categories);
        Assert.Equal(ProductKind.Membership, products[4].Kind);
        Assert.Equal(new[] { ProductCategory.Book, ProductCategory.Video }, products[4].Categories);
    }

    [SqlServerFact]
    public async Task A_product_can_be_fetched_by_id()
    {
        await using var factory = new ApiFactory(fixture.NewConnectionString());
        using var client = factory.CreateClient();

        var product = await ApiFactory.ReadAsync<ProductResponse>(await client.GetAsync("/api/v1/products/1"));

        Assert.Equal((1L, "Comprehensive First Aid Training", 24.00m, ProductKind.Digital), (product.Id, product.Name, product.Price, product.Kind));
        Assert.Equal(new[] { ProductCategory.Video }, product.Categories);
    }

    // Reads the raw JSON, because the test client's own enum converter would also accept numbers.
    [SqlServerFact]
    public async Task Property_names_are_camelCase_and_enums_are_written_as_names()
    {
        await using var factory = new ApiFactory(fixture.NewConnectionString());
        using var client = factory.CreateClient();

        var product = await ApiFactory.ReadAsync<JsonElement>(await client.GetAsync("/api/v1/products/2"));

        Assert.Equal("Physical", product.GetProperty("kind").GetString());
        Assert.Equal("Book", product.GetProperty("categories")[0].GetString());
    }

    [SqlServerFact]
    public async Task An_unknown_product_returns_404()
    {
        await using var factory = new ApiFactory(fixture.NewConnectionString());
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/v1/products/999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [SqlServerFact]
    public async Task The_demo_customer_starts_with_no_memberships_and_no_access()
    {
        await using var factory = new ApiFactory(fixture.NewConnectionString());
        using var client = factory.CreateClient();

        var customer = await ApiFactory.ReadAsync<CustomerResponse>(await client.GetAsync("/api/v1/customers/4567890"));

        Assert.Equal("Demo Customer", customer.Name);
        Assert.Empty(customer.Memberships);
        Assert.Empty(customer.AccessibleCategories);
    }

    [SqlServerFact]
    public async Task An_unknown_customer_returns_404()
    {
        await using var factory = new ApiFactory(fixture.NewConnectionString());
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/v1/customers/12345");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [SqlServerFact]
    public async Task An_unknown_route_returns_a_problem_details_404()
    {
        await using var factory = new ApiFactory(fixture.NewConnectionString());
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/v1/nope");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [SqlServerFact]
    public async Task The_OpenAPI_document_describes_enums_by_name()
    {
        await using var factory = new ApiFactory(fixture.NewConnectionString());
        using var client = factory.CreateClient();

        var document = await client.GetStringAsync("/openapi/v1.json");

        Assert.Contains("\"Physical\"", document);
        Assert.Contains("\"Membership\"", document);
    }

    [SqlServerFact]
    public async Task The_OpenAPI_document_carries_the_summaries_written_on_the_controllers()
    {
        await using var factory = new ApiFactory(fixture.NewConnectionString());
        using var client = factory.CreateClient();

        var document = await client.GetStringAsync("/openapi/v1.json");

        Assert.Contains("Places a purchase order for a customer", document);
    }

    [SqlServerFact]
    public async Task The_OpenAPI_document_lists_every_endpoint()
    {
        await using var factory = new ApiFactory(fixture.NewConnectionString());
        using var client = factory.CreateClient();

        var document = await ApiFactory.ReadAsync<JsonElement>(await client.GetAsync("/openapi/v1.json"));

        var paths = document.GetProperty("paths").EnumerateObject().Select(path => path.Name).Order().ToList();
        Assert.Equal(
            new[]
            {
                "/api/v1/customers",
                "/api/v1/customers/{id}",
                "/api/v1/products",
                "/api/v1/products/{id}",
                "/api/v1/purchase-orders",
                "/api/v1/purchase-orders/{id}",
                "/api/v1/purchase-orders/{id}/shipping-slip",
            },
            paths);
    }
}
