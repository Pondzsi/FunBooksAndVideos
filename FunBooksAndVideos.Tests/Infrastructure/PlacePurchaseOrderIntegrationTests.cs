using FunBooksAndVideos.Application.Orders;
using FunBooksAndVideos.Application.Products;
using FunBooksAndVideos.Domain.Products;
using FunBooksAndVideos.Domain.Shipping;
using FunBooksAndVideos.Infrastructure.Persistence;
using FunBooksAndVideos.Tests.Application;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FunBooksAndVideos.Tests.Infrastructure;

// The whole flow on the real services and a real SQL Server, against the seeded catalog.
[Collection(SqlServerCollection.Name)]
public class PlacePurchaseOrderIntegrationTests(SqlServerFixture fixture)
{
    [SqlServerFact]
    public async Task Places_the_PDF_example_order_and_persists_everything_together()
    {
        var sink = new LogSink();
        await using var provider = await fixture.CreateSeededDatabaseAsync(sink);

        var result = await PlaceAsync(provider, 4567890, 1, 2, 3);

        Assert.Equal(48.50m, result.Order.Total);

        await using var scope = provider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var customer = await context.Customers.SingleAsync(c => c.Id == 4567890);
        var membership = Assert.Single(customer.Memberships);
        Assert.Equal(3, membership.ProductId);
        Assert.True(customer.HasAccessTo(ProductCategory.Book));
        Assert.False(customer.HasAccessTo(ProductCategory.Video));

        var order = await context.PurchaseOrders.Include(o => o.Items).SingleAsync(o => o.Id == result.Order.Id);
        Assert.Equal(new[] { 9.50m, 15.00m, 24.00m }, order.Items.Select(item => item.Price).Order());

        var slip = await context.ShippingSlips.SingleAsync(s => s.PurchaseOrderId == order.Id);
        Assert.Equal(new ShippingSlipItem(2, "The Girl on the train"), Assert.Single(slip.Items));

        Assert.Contains("Membership 3 activated for customer 4567890.", sink.Messages);
        Assert.Contains($"Shipping slip generated for order {order.Id} (customer 4567890).", sink.Messages);
    }

    [SqlServerFact]
    public async Task Ordering_the_same_membership_twice_keeps_a_single_membership()
    {
        await using var provider = await fixture.CreateSeededDatabaseAsync();

        await PlaceAsync(provider, 4567890, 3);
        await PlaceAsync(provider, 4567890, 3);

        await using var scope = provider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var customer = await context.Customers.SingleAsync(c => c.Id == 4567890);
        Assert.Single(customer.Memberships);
        Assert.Equal(2, await context.PurchaseOrders.CountAsync());
    }

    [SqlServerFact]
    public async Task An_unknown_product_saves_nothing()
    {
        var sink = new LogSink();
        await using var provider = await fixture.CreateSeededDatabaseAsync(sink);

        await Assert.ThrowsAsync<ProductNotFoundException>(() => PlaceAsync(provider, 4567890, 3, 2, 99));

        await using var scope = provider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var customer = await context.Customers.SingleAsync(c => c.Id == 4567890);
        Assert.Empty(customer.Memberships);
        Assert.Equal(0, await context.PurchaseOrders.CountAsync());
        Assert.Equal(0, await context.ShippingSlips.CountAsync());
        Assert.Empty(sink.Messages);
    }

    private static async Task<PlacePurchaseOrderResult> PlaceAsync(ServiceProvider provider, long customerId, params long[] productIds)
    {
        await using var scope = provider.CreateAsyncScope();

        return await scope.ServiceProvider.GetRequiredService<PlacePurchaseOrder>()
            .ExecuteAsync(new PlacePurchaseOrderCommand(customerId, productIds), CancellationToken.None);
    }
}
