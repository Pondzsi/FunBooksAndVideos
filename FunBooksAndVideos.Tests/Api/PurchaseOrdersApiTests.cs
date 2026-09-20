using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FunBooksAndVideos.Api.Contracts.Customers;
using FunBooksAndVideos.Api.Contracts.Products;
using FunBooksAndVideos.Api.Contracts.PurchaseOrders;
using FunBooksAndVideos.Api.Contracts.Shipping;
using FunBooksAndVideos.Application.Products;
using FunBooksAndVideos.Domain.Products;
using FunBooksAndVideos.Tests.Infrastructure;

namespace FunBooksAndVideos.Tests.Api;

// Black-box tests over HTTP against the seeded catalog: products 1 to 3 and customer 4567890 are the PDF's example.
[Collection(SqlServerCollection.Name)]
public class PurchaseOrdersApiTests(SqlServerFixture fixture)
{
    [SqlServerFact]
    public async Task Placing_the_PDF_example_order_returns_201_with_a_location_and_the_priced_order()
    {
        await using var factory = new ApiFactory(fixture.NewConnectionString());
        using var client = factory.CreateClient();

        var response = await ApiFactory.PlaceOrderAsync(client, 4567890, 1, 2, 3);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var order = await ApiFactory.ReadAsync<PurchaseOrderResponse>(response);
        Assert.Equal(4567890, order.CustomerId);
        Assert.Equal(48.50m, order.Total);
        Assert.Equal(new[] { 24.00m, 9.50m, 15.00m }, order.Items.Select(item => item.Price));
        Assert.Equal(
            new[] { ProductKind.Digital, ProductKind.Physical, ProductKind.Membership },
            order.Items.Select(item => item.Kind));
        Assert.EndsWith($"/api/v1/purchase-orders/{order.Id}", response.Headers.Location!.ToString());
    }

    [SqlServerFact]
    public async Task Placing_the_PDF_example_order_reports_what_the_rules_did()
    {
        await using var factory = new ApiFactory(fixture.NewConnectionString());
        using var client = factory.CreateClient();

        var order = await ApiFactory.ReadAsync<PlacedPurchaseOrderResponse>(await ApiFactory.PlaceOrderAsync(client, 4567890, 1, 2, 3));

        var membership = Assert.Single(order.ActivatedMemberships);
        Assert.Equal(3, membership.ProductId);
        Assert.Equal(new[] { ProductCategory.Book }, membership.Categories);
        Assert.True(order.ShippingSlipGenerated);
        Assert.Equal(
            new[] { ProductCategory.Video, ProductCategory.Book, ProductCategory.Book },
            order.Items.Select(item => Assert.Single(item.Categories)));
    }

    [SqlServerFact]
    public async Task An_order_of_a_video_and_a_membership_reports_no_shipping_slip()
    {
        await using var factory = new ApiFactory(fixture.NewConnectionString());
        using var client = factory.CreateClient();

        var order = await ApiFactory.ReadAsync<PlacedPurchaseOrderResponse>(await ApiFactory.PlaceOrderAsync(client, 4567890, 1, 3));

        Assert.False(order.ShippingSlipGenerated);
        Assert.Single(order.ActivatedMemberships);
    }

    [SqlServerFact]
    public async Task Ordering_a_membership_the_customer_already_holds_activates_nothing()
    {
        await using var factory = new ApiFactory(fixture.NewConnectionString());
        using var client = factory.CreateClient();
        await ApiFactory.PlaceOrderAsync(client, 4567890, 3);

        var second = await ApiFactory.ReadAsync<PlacedPurchaseOrderResponse>(await ApiFactory.PlaceOrderAsync(client, 4567890, 3));

        Assert.Empty(second.ActivatedMemberships);
    }

    [SqlServerFact]
    public async Task Orders_are_listed_oldest_first_and_can_be_filtered_by_customer()
    {
        await using var factory = new ApiFactory(fixture.NewConnectionString());
        using var client = factory.CreateClient();
        var otherCustomer = await ApiFactory.ReadAsync<CustomerResponse>(
            await client.PostAsJsonAsync("/api/v1/customers", new { name = "Grace Hopper" }, ApiFactory.Json));
        var first = await ApiFactory.ReadAsync<PlacedPurchaseOrderResponse>(await ApiFactory.PlaceOrderAsync(client, 4567890, 1));
        var second = await ApiFactory.ReadAsync<PlacedPurchaseOrderResponse>(await ApiFactory.PlaceOrderAsync(client, otherCustomer.Id, 2));

        var all = await ApiFactory.ReadAsync<List<PurchaseOrderResponse>>(await client.GetAsync("/api/v1/purchase-orders"));
        var mine = await ApiFactory.ReadAsync<List<PurchaseOrderResponse>>(await client.GetAsync($"/api/v1/purchase-orders?customerId={otherCustomer.Id}"));
        var nobody = await ApiFactory.ReadAsync<List<PurchaseOrderResponse>>(await client.GetAsync("/api/v1/purchase-orders?customerId=999"));

        Assert.Equal(new[] { first.Id, second.Id }, all.Select(order => order.Id));
        Assert.Equal(second.Id, Assert.Single(mine).Id);
        Assert.Empty(nobody);
    }

    [SqlServerFact]
    public async Task The_location_returns_the_placed_order()
    {
        await using var factory = new ApiFactory(fixture.NewConnectionString());
        using var client = factory.CreateClient();
        var created = await ApiFactory.PlaceOrderAsync(client, 4567890, 1, 2, 3);

        var response = await client.GetAsync(created.Headers.Location);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var order = await ApiFactory.ReadAsync<PurchaseOrderResponse>(response);
        Assert.Equal(48.50m, order.Total);
        Assert.Equal(new[] { 9.50m, 15.00m, 24.00m }, order.Items.Select(item => item.Price).Order());
    }

    [SqlServerFact]
    public async Task Placing_the_order_activates_the_membership_on_the_customer()
    {
        await using var factory = new ApiFactory(fixture.NewConnectionString());
        using var client = factory.CreateClient();
        await ApiFactory.PlaceOrderAsync(client, 4567890, 1, 2, 3);

        var customer = await ApiFactory.ReadAsync<CustomerResponse>(await client.GetAsync("/api/v1/customers/4567890"));

        var membership = Assert.Single(customer.Memberships);
        Assert.Equal(3, membership.ProductId);
        Assert.Equal(new[] { ProductCategory.Book }, membership.Categories);
        Assert.Equal(new[] { ProductCategory.Book }, customer.AccessibleCategories);
    }

    [SqlServerFact]
    public async Task Placing_the_order_generates_a_shipping_slip_with_the_physical_products_only()
    {
        await using var factory = new ApiFactory(fixture.NewConnectionString());
        using var client = factory.CreateClient();
        var order = await ApiFactory.ReadAsync<PurchaseOrderResponse>(await ApiFactory.PlaceOrderAsync(client, 4567890, 1, 2, 3));

        var response = await client.GetAsync($"/api/v1/purchase-orders/{order.Id}/shipping-slip");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var slip = await ApiFactory.ReadAsync<ShippingSlipResponse>(response);
        Assert.Equal(order.Id, slip.PurchaseOrderId);
        Assert.Equal(4567890, slip.CustomerId);
        Assert.Equal(new ShippingSlipItemResponse(2, "The Girl on the train"), Assert.Single(slip.Items));
    }

    [SqlServerFact]
    public async Task An_order_of_a_video_and_a_membership_has_no_shipping_slip()
    {
        await using var factory = new ApiFactory(fixture.NewConnectionString());
        using var client = factory.CreateClient();
        var order = await ApiFactory.ReadAsync<PurchaseOrderResponse>(await ApiFactory.PlaceOrderAsync(client, 4567890, 1, 3));

        var response = await client.GetAsync($"/api/v1/purchase-orders/{order.Id}/shipping-slip");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [SqlServerFact]
    public async Task Ordering_the_same_membership_twice_succeeds_and_keeps_one_membership()
    {
        await using var factory = new ApiFactory(fixture.NewConnectionString());
        using var client = factory.CreateClient();

        var first = await ApiFactory.PlaceOrderAsync(client, 4567890, 3);
        var second = await ApiFactory.PlaceOrderAsync(client, 4567890, 3);

        Assert.Equal(HttpStatusCode.Created, first.StatusCode);
        Assert.Equal(HttpStatusCode.Created, second.StatusCode);
        var customer = await ApiFactory.ReadAsync<CustomerResponse>(await client.GetAsync("/api/v1/customers/4567890"));
        Assert.Single(customer.Memberships);
    }

    [SqlServerFact]
    public async Task An_unknown_product_returns_422_naming_it_and_saves_nothing()
    {
        await using var factory = new ApiFactory(fixture.NewConnectionString());
        using var client = factory.CreateClient();

        var response = await ApiFactory.PlaceOrderAsync(client, 4567890, 3, 99);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var problem = await ApiFactory.ReadAsync<JsonElement>(response);
        Assert.Equal(422, problem.GetProperty("status").GetInt32());
        Assert.Equal(99, problem.GetProperty("productId").GetInt64());
        var customer = await ApiFactory.ReadAsync<CustomerResponse>(await client.GetAsync("/api/v1/customers/4567890"));
        Assert.Empty(customer.Memberships);
    }

    [SqlServerFact]
    public async Task An_unknown_customer_returns_422_naming_them()
    {
        await using var factory = new ApiFactory(fixture.NewConnectionString());
        using var client = factory.CreateClient();

        var response = await ApiFactory.PlaceOrderAsync(client, 12345, 1);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        var problem = await ApiFactory.ReadAsync<JsonElement>(response);
        Assert.Equal(12345, problem.GetProperty("customerId").GetInt64());
    }

    [SqlServerFact]
    public async Task An_order_without_products_returns_400_with_the_field_error()
    {
        await using var factory = new ApiFactory(fixture.NewConnectionString());
        using var client = factory.CreateClient();

        var response = await ApiFactory.PlaceOrderAsync(client, 4567890);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var problem = await ApiFactory.ReadAsync<JsonElement>(response);
        Assert.Equal(
            "ProductIds must contain at least one product ID.",
            problem.GetProperty("errors").GetProperty("ProductIds")[0].GetString());
    }

    [SqlServerFact]
    public async Task A_body_without_fields_returns_400_for_each_missing_field()
    {
        await using var factory = new ApiFactory(fixture.NewConnectionString());
        using var client = factory.CreateClient();

        var response = await client.PostAsync("/api/v1/purchase-orders", new StringContent("{}", System.Text.Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errors = (await ApiFactory.ReadAsync<JsonElement>(response)).GetProperty("errors");
        Assert.True(errors.TryGetProperty("CustomerId", out _));
        Assert.True(errors.TryGetProperty("ProductIds", out _));
    }

    [SqlServerFact]
    public async Task An_unknown_order_returns_404_problem_details()
    {
        await using var factory = new ApiFactory(fixture.NewConnectionString());
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/v1/purchase-orders/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var problem = await ApiFactory.ReadAsync<JsonElement>(response);
        Assert.Equal("Purchase order 999999 was not found.", problem.GetProperty("detail").GetString());
    }
}
