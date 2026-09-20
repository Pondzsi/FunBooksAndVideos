using FunBooksAndVideos.Application.Common;
using FunBooksAndVideos.Application.Orders;
using FunBooksAndVideos.Application.Shipping;
using FunBooksAndVideos.Domain.Orders;
using FunBooksAndVideos.Domain.Products;
using FunBooksAndVideos.Domain.Shipping;
using FunBooksAndVideos.Infrastructure;
using FunBooksAndVideos.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FunBooksAndVideos.Tests.Infrastructure;

// Every test reads back through a new scope, so it sees what the database holds and not what EF has tracked.
[Collection(SqlServerCollection.Name)]
public class PersistenceTests(SqlServerFixture fixture)
{
    [SqlServerFact]
    public async Task Initializing_migrates_and_seeds_the_demo_catalog_and_customer()
    {
        await using var provider = await fixture.CreateSeededDatabaseAsync();
        await using var scope = provider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        Assert.Equal(7, await context.Products.CountAsync());
        Assert.Equal(4567890, await context.Customers.Select(customer => customer.Id).SingleAsync());
    }

    [SqlServerFact]
    public async Task Initializing_again_does_not_duplicate_the_seed_data()
    {
        await using var provider = await fixture.CreateSeededDatabaseAsync();

        await provider.InitializeDatabaseAsync();

        await using var scope = provider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.Equal(7, await context.Products.CountAsync());
        Assert.Equal(1, await context.Customers.CountAsync());
    }

    [SqlServerFact]
    public async Task Products_come_back_as_their_own_types_with_their_categories()
    {
        await using var provider = await fixture.CreateSeededDatabaseAsync();
        await using var scope = provider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var products = await context.Products.OrderBy(product => product.Id).ToListAsync();

        var video = Assert.IsType<DigitalProduct>(products[0]);
        Assert.Equal("Comprehensive First Aid Training", video.Name);
        Assert.Equal(24.00m, video.Price);
        Assert.Equal(ProductCategory.Video, video.Category);

        var book = Assert.IsType<PhysicalProduct>(products[1]);
        Assert.Equal(ProductCategory.Book, book.Category);

        var premium = Assert.IsType<MembershipProduct>(products[4]);
        Assert.Equal(new[] { ProductCategory.Book, ProductCategory.Video }, premium.GrantedCategories.Order());
    }

    [SqlServerFact]
    public async Task A_membership_comes_back_with_its_categories_and_activation_time()
    {
        await using var provider = await fixture.CreateSeededDatabaseAsync();

        await using (var scope = provider.CreateAsyncScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var customer = await context.Customers.SingleAsync(c => c.Id == 4567890);
            var premium = Assert.IsType<MembershipProduct>(await context.Products.SingleAsync(p => p.Id == 5));

            customer.ActivateMembership(premium, TestData.Now);
            await context.SaveChangesAsync();
        }

        await using (var scope = provider.CreateAsyncScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var customer = await context.Customers.SingleAsync(c => c.Id == 4567890);

            var membership = Assert.Single(customer.Memberships);
            Assert.Equal(5, membership.ProductId);
            Assert.Equal(new[] { ProductCategory.Book, ProductCategory.Video }, membership.GrantedCategories.Order());
            Assert.Equal(TestData.Now, membership.ActivatedAt);
            Assert.True(customer.HasAccessTo(ProductCategory.Video));
        }
    }

    [SqlServerFact]
    public async Task The_database_rejects_a_second_membership_for_the_same_product()
    {
        await using var provider = await fixture.CreateSeededDatabaseAsync();
        await using var scope = provider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        const string insert = "INSERT INTO Memberships (CustomerId, ProductId, GrantedCategories, ActivatedAt) VALUES (4567890, 3, 'Book', SYSDATETIMEOFFSET())";

        await context.Database.ExecuteSqlRawAsync(insert);

        await Assert.ThrowsAsync<SqlException>(() => context.Database.ExecuteSqlRawAsync(insert));
    }

    [SqlServerFact]
    public async Task Purchase_order_ids_come_from_an_increasing_sequence()
    {
        await using var provider = await fixture.CreateSeededDatabaseAsync();
        await using var scope = provider.CreateAsyncScope();
        var orders = scope.ServiceProvider.GetRequiredService<IPurchaseOrderRepository>();

        var first = await orders.NextIdAsync(CancellationToken.None);
        var second = await orders.NextIdAsync(CancellationToken.None);

        Assert.True(first > 0);
        Assert.Equal(first + 1, second);
    }

    [SqlServerFact]
    public async Task A_purchase_order_keeps_the_price_each_item_had_when_it_was_placed()
    {
        await using var provider = await fixture.CreateSeededDatabaseAsync();
        long orderId;

        await using (var scope = provider.CreateAsyncScope())
        {
            var order = await StageOrderAsync(scope.ServiceProvider, 1, 2, 3);
            orderId = order.Id;
            await scope.ServiceProvider.GetRequiredService<IUnitOfWork>().CommitAsync(CancellationToken.None);

            // The catalog price of the book changes afterwards.
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await context.Database.ExecuteSqlRawAsync("UPDATE Products SET Price = 99.00 WHERE Id = 2");
        }

        await using (var scope = provider.CreateAsyncScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var order = await context.PurchaseOrders.Include(o => o.Items).ThenInclude(i => i.Product).SingleAsync(o => o.Id == orderId);

            Assert.Equal(48.50m, order.Total);
            var book = Assert.Single(order.Items, item => item.Product.Id == 2);
            Assert.Equal(9.50m, book.Price);
            Assert.Equal(99.00m, book.Product.Price);
        }
    }

    [SqlServerFact]
    public async Task A_shipping_slip_comes_back_with_its_lines_and_generation_time()
    {
        await using var provider = await fixture.CreateSeededDatabaseAsync();
        long orderId;

        await using (var scope = provider.CreateAsyncScope())
        {
            var order = await StageOrderAsync(scope.ServiceProvider, 1, 2, 3);
            orderId = order.Id;
            var slips = scope.ServiceProvider.GetRequiredService<IShippingSlipRepository>();
            await slips.AddAsync(ShippingSlip.Generate(order, TestData.Now), CancellationToken.None);
            await scope.ServiceProvider.GetRequiredService<IUnitOfWork>().CommitAsync(CancellationToken.None);
        }

        await using (var scope = provider.CreateAsyncScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var slip = await context.ShippingSlips.SingleAsync(s => s.PurchaseOrderId == orderId);

            Assert.Equal(4567890, slip.CustomerId);
            Assert.Equal(TestData.Now, slip.GeneratedAt);
            Assert.Equal(new ShippingSlipItem(2, "The Girl on the train"), Assert.Single(slip.Items));
        }
    }

    [SqlServerFact]
    public async Task A_failed_commit_saves_nothing()
    {
        await using var provider = await fixture.CreateSeededDatabaseAsync();

        await using (var scope = provider.CreateAsyncScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var customer = await context.Customers.SingleAsync(c => c.Id == 4567890);
            var premium = Assert.IsType<MembershipProduct>(await context.Products.SingleAsync(p => p.Id == 5));
            customer.ActivateMembership(premium, TestData.Now);

            // A slip for an order that was never saved breaks the foreign key, so the whole commit must fail.
            var unsavedOrder = new PurchaseOrder(999_999, customer.Id, [TestData.GirlOnTheTrain]);
            context.ShippingSlips.Add(ShippingSlip.Generate(unsavedOrder, TestData.Now));

            await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
        }

        await using (var scope = provider.CreateAsyncScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var customer = await context.Customers.SingleAsync(c => c.Id == 4567890);

            Assert.Empty(customer.Memberships);
        }
    }

    // Builds an order for the seeded customer from seeded products and stages it, without committing.
    private static async Task<PurchaseOrder> StageOrderAsync(IServiceProvider services, params long[] productIds)
    {
        var context = services.GetRequiredService<AppDbContext>();
        var orders = services.GetRequiredService<IPurchaseOrderRepository>();
        var products = await context.Products.Where(product => productIds.Contains(product.Id)).ToListAsync();

        var order = new PurchaseOrder(await orders.NextIdAsync(CancellationToken.None), 4567890, products);
        await orders.AddAsync(order, CancellationToken.None);

        return order;
    }
}
