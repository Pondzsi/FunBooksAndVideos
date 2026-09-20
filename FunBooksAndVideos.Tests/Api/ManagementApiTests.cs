using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using FunBooksAndVideos.Api.Contracts.Customers;
using FunBooksAndVideos.Api.Contracts.Products;
using FunBooksAndVideos.Api.Contracts.PurchaseOrders;
using FunBooksAndVideos.Application.Products;
using FunBooksAndVideos.Domain.Products;
using FunBooksAndVideos.Tests.Infrastructure;

namespace FunBooksAndVideos.Tests.Api;

// Creating customers and catalog products over HTTP, and using them.
[Collection(SqlServerCollection.Name)]
public class ManagementApiTests(SqlServerFixture fixture)
{
    [SqlServerFact]
    public async Task Creating_a_customer_returns_201_with_a_location_and_no_memberships()
    {
        await using var factory = new ApiFactory(fixture.NewConnectionString());
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/customers", new { name = "Ada Lovelace" }, ApiFactory.Json);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var customer = await ApiFactory.ReadAsync<CustomerResponse>(response);
        Assert.Equal("Ada Lovelace", customer.Name);
        Assert.Empty(customer.Memberships);
        Assert.EndsWith($"/api/v1/customers/{customer.Id}", response.Headers.Location!.ToString());
        var fetched = await ApiFactory.ReadAsync<CustomerResponse>(await client.GetAsync(response.Headers.Location));
        Assert.Equal(customer.Id, fetched.Id);
    }

    [SqlServerFact]
    public async Task Customers_are_listed_with_the_seeded_one_first()
    {
        await using var factory = new ApiFactory(fixture.NewConnectionString());
        using var client = factory.CreateClient();
        await client.PostAsJsonAsync("/api/v1/customers", new { name = "Ada Lovelace" }, ApiFactory.Json);

        var customers = await ApiFactory.ReadAsync<List<CustomerResponse>>(await client.GetAsync("/api/v1/customers"));

        Assert.Equal(new[] { "Ada Lovelace", "Demo Customer" }, customers.Select(customer => customer.Name).Order());
        Assert.Equal(2, customers.Count);
    }

    [SqlServerFact]
    public async Task A_new_customer_can_place_an_order_and_gets_the_membership()
    {
        await using var factory = new ApiFactory(fixture.NewConnectionString());
        using var client = factory.CreateClient();
        var customer = await ApiFactory.ReadAsync<CustomerResponse>(
            await client.PostAsJsonAsync("/api/v1/customers", new { name = "Ada Lovelace" }, ApiFactory.Json));

        var order = await ApiFactory.ReadAsync<PlacedPurchaseOrderResponse>(await ApiFactory.PlaceOrderAsync(client, customer.Id, 5));

        Assert.Equal(customer.Id, order.CustomerId);
        Assert.Equal(new[] { ProductCategory.Book, ProductCategory.Video }, Assert.Single(order.ActivatedMemberships).Categories);
    }

    [SqlServerTheory]
    [InlineData("{}")]
    [InlineData("""{"name": ""}""")]
    [InlineData("""{"name": "   "}""")]
    public async Task A_customer_needs_a_name(string body)
    {
        await using var factory = new ApiFactory(fixture.NewConnectionString());
        using var client = factory.CreateClient();

        var response = await client.PostAsync("/api/v1/customers", new StringContent(body, Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.True((await ApiFactory.ReadAsync<JsonElement>(response)).GetProperty("errors").TryGetProperty("Name", out _));
    }

    [SqlServerFact]
    public async Task Creating_a_physical_product_returns_201_and_adds_it_to_the_catalog()
    {
        await using var factory = new ApiFactory(fixture.NewConnectionString());
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/v1/products",
            new { kind = "Physical", name = "Refactoring", price = 39.99m, categories = new[] { "Book" } },
            ApiFactory.Json);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var product = await ApiFactory.ReadAsync<ProductResponse>(response);
        Assert.True(product.Id >= 100, "new products are numbered above the seeded 1 to 7");
        Assert.Equal((ProductKind.Physical, "Refactoring", 39.99m), (product.Kind, product.Name, product.Price));
        Assert.Equal(new[] { ProductCategory.Book }, product.Categories);
        Assert.EndsWith($"/api/v1/products/{product.Id}", response.Headers.Location!.ToString());
        var catalog = await ApiFactory.ReadAsync<List<ProductResponse>>(await client.GetAsync("/api/v1/products"));
        Assert.Equal(8, catalog.Count);
    }

    [SqlServerFact]
    public async Task A_new_single_category_membership_can_be_ordered_and_grants_only_that_category()
    {
        await using var factory = new ApiFactory(fixture.NewConnectionString());
        using var client = factory.CreateClient();
        var membership = await ApiFactory.ReadAsync<ProductResponse>(await client.PostAsJsonAsync(
            "/api/v1/products",
            new { kind = "Membership", name = "Video Club Plus", price = 12m, categories = new[] { "Video" } },
            ApiFactory.Json));

        await ApiFactory.PlaceOrderAsync(client, 4567890, membership.Id);

        var customer = await ApiFactory.ReadAsync<CustomerResponse>(await client.GetAsync("/api/v1/customers/4567890"));
        Assert.Equal(new[] { ProductCategory.Video }, customer.AccessibleCategories);
    }

    [SqlServerTheory]
    [InlineData("""{"kind": "Physical", "name": "Two shelves", "price": 10, "categories": ["Book", "Video"]}""", "Categories")]
    [InlineData("""{"kind": "Digital", "name": "No shelf", "price": 10, "categories": []}""", "Categories")]
    [InlineData("""{"kind": "Membership", "name": "Nothing", "price": 10, "categories": []}""", "Categories")]
    [InlineData("""{"kind": "Digital", "name": "Free", "price": 0, "categories": ["Video"]}""", "Price")]
    [InlineData("""{"kind": "Digital", "name": "", "price": 10, "categories": ["Video"]}""", "Name")]
    [InlineData("""{"name": "No kind", "price": 10, "categories": ["Video"]}""", "Kind")]
    public async Task An_invalid_product_is_rejected_with_the_field_named(string body, string field)
    {
        await using var factory = new ApiFactory(fixture.NewConnectionString());
        using var client = factory.CreateClient();

        var response = await client.PostAsync("/api/v1/products", new StringContent(body, Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        Assert.True((await ApiFactory.ReadAsync<JsonElement>(response)).GetProperty("errors").TryGetProperty(field, out _));
    }

    [SqlServerTheory]
    [InlineData("""{"kind": "Gadget", "name": "Unknown kind", "price": 10, "categories": ["Video"]}""")]
    [InlineData("""{"kind": 1, "name": "A number for the kind", "price": 10, "categories": ["Video"]}""")]
    [InlineData("""{"kind": "Digital", "name": "Unknown category", "price": 10, "categories": ["Music"]}""")]
    public async Task A_number_or_an_unknown_name_for_an_enum_is_rejected(string body)
    {
        await using var factory = new ApiFactory(fixture.NewConnectionString());
        using var client = factory.CreateClient();

        var response = await client.PostAsync("/api/v1/products", new StringContent(body, Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
