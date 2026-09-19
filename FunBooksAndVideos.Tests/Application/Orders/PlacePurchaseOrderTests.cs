using FunBooksAndVideos.Application.Customers;
using FunBooksAndVideos.Application.Orders;
using FunBooksAndVideos.Application.Orders.Processing;
using FunBooksAndVideos.Application.Products;
using FunBooksAndVideos.Application.Shipping;
using FunBooksAndVideos.Domain.Customers;
using FunBooksAndVideos.Domain.Orders;
using FunBooksAndVideos.Domain.Products;
using FunBooksAndVideos.Domain.Shipping;
using MediatR;

namespace FunBooksAndVideos.Tests.Application.Orders;

public class PlacePurchaseOrderTests
{
    [Fact]
    public async Task Places_the_PDF_example_order()
    {
        var fixture = new Fixture();

        var result = await fixture.UseCase.ExecuteAsync(new(4567890, [1, 2, 3]), CancellationToken.None);

        Assert.Equal(3344656, result.Order.Id);
        Assert.Equal(48.50m, result.Order.Total);
        Assert.Same(result.Order, Assert.Single(fixture.Orders.Added));
        Assert.Same(fixture.Customer, Assert.Single(fixture.Customers.Saved));
        Assert.True(fixture.Customer.HasAccessTo(ProductCategory.Book));
        Assert.NotNull(result.ShippingSlip);
        Assert.Same(result.ShippingSlip, Assert.Single(fixture.Slips.Added));
    }

    [Fact]
    public async Task Publishes_the_domain_events_only_after_everything_is_saved()
    {
        var fixture = new Fixture();

        await fixture.UseCase.ExecuteAsync(new(4567890, [1, 2, 3]), CancellationToken.None);

        var firstPublish = fixture.Calls.FindIndex(call => call.StartsWith("publish:"));
        var lastSave = fixture.Calls.FindLastIndex(call => call.StartsWith("save:"));
        Assert.True(firstPublish > lastSave);
        Assert.Contains("publish:MembershipActivated", fixture.Calls);
        Assert.Contains("publish:ShippingSlipGenerated", fixture.Calls);
    }

    [Fact]
    public async Task Clears_the_domain_events_it_published()
    {
        var fixture = new Fixture();

        var result = await fixture.UseCase.ExecuteAsync(new(4567890, [1, 2, 3]), CancellationToken.None);

        Assert.Empty(fixture.Customer.DomainEvents);
        Assert.Empty(result.ShippingSlip!.DomainEvents);
    }

    [Fact]
    public async Task Saves_no_shipping_slip_when_the_order_has_no_physical_product()
    {
        var fixture = new Fixture();

        var result = await fixture.UseCase.ExecuteAsync(new(4567890, [1, 3]), CancellationToken.None);

        Assert.Null(result.ShippingSlip);
        Assert.Empty(fixture.Slips.Added);
        Assert.DoesNotContain("publish:ShippingSlipGenerated", fixture.Calls);
    }

    [Fact]
    public async Task Uses_the_time_from_the_time_provider()
    {
        var fixture = new Fixture();

        var result = await fixture.UseCase.ExecuteAsync(new(4567890, [1, 2, 3]), CancellationToken.None);

        Assert.Equal(TestData.Now, Assert.Single(fixture.Customer.Memberships).ActivatedAt);
        Assert.Equal(TestData.Now, result.ShippingSlip!.GeneratedAt);
    }

    [Fact]
    public async Task Fails_for_an_unknown_customer_without_saving_or_publishing()
    {
        var fixture = new Fixture();

        var exception = await Assert.ThrowsAsync<CustomerNotFoundException>(
            () => fixture.UseCase.ExecuteAsync(new(999, [1]), CancellationToken.None));

        Assert.Equal(999, exception.CustomerId);
        Assert.Empty(fixture.Calls);
    }

    [Fact]
    public async Task Fails_for_an_unknown_product_without_saving_or_publishing()
    {
        var fixture = new Fixture();

        var exception = await Assert.ThrowsAsync<ProductNotFoundException>(
            () => fixture.UseCase.ExecuteAsync(new(4567890, [1, 99]), CancellationToken.None));

        Assert.Equal(99, exception.ProductId);
        Assert.Empty(fixture.Calls);
        Assert.Empty(fixture.Customer.Memberships);
    }

    [Fact]
    public async Task Rejects_an_order_without_products_without_saving_or_publishing()
    {
        var fixture = new Fixture();

        await Assert.ThrowsAsync<ArgumentException>(
            () => fixture.UseCase.ExecuteAsync(new(4567890, []), CancellationToken.None));

        Assert.Empty(fixture.Calls);
    }

    private sealed class Fixture
    {
        public Fixture()
        {
            Customers = new FakeCustomers(Calls);
            Orders = new FakeOrders(Calls);
            Slips = new FakeShippingSlips(Calls);

            Customers.Store[Customer.Id] = Customer;

            foreach (var product in new[] { TestData.FirstAidVideo, TestData.GirlOnTheTrain, TestData.BookClub })
            {
                Products.Store[product.Id] = product;
            }

            var processor = new PurchaseOrderProcessor([new ActivateMembershipRule(), new GenerateShippingSlipRule()]);

            UseCase = new PlacePurchaseOrder(
                Customers,
                Products,
                Orders,
                Slips,
                processor,
                new FakePublisher(Calls),
                new FixedTimeProvider(TestData.Now));
        }

        // "save:..." and "publish:..." entries, in the order they happened.
        public List<string> Calls { get; } = [];

        public Customer Customer { get; } = TestData.NewCustomer();

        public FakeCustomers Customers { get; }

        public FakeProducts Products { get; } = new();

        public FakeOrders Orders { get; }

        public FakeShippingSlips Slips { get; }

        public PlacePurchaseOrder UseCase { get; }
    }

    private sealed class FakeCustomers(List<string> calls) : ICustomerRepository
    {
        public Dictionary<long, Customer> Store { get; } = [];

        public List<Customer> Saved { get; } = [];

        public Task<Customer?> GetByIdAsync(long id, CancellationToken cancellationToken)
        {
            return Task.FromResult(Store.GetValueOrDefault(id));
        }

        public Task SaveAsync(Customer customer, CancellationToken cancellationToken)
        {
            calls.Add("save:customer");
            Saved.Add(customer);

            return Task.CompletedTask;
        }
    }

    private sealed class FakeProducts : IProductRepository
    {
        public Dictionary<long, Product> Store { get; } = [];

        public Task<Product?> GetByIdAsync(long id, CancellationToken cancellationToken)
        {
            return Task.FromResult(Store.GetValueOrDefault(id));
        }
    }

    private sealed class FakeOrders(List<string> calls) : IPurchaseOrderRepository
    {
        public List<PurchaseOrder> Added { get; } = [];

        public Task<long> NextIdAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(3344656L);
        }

        public Task AddAsync(PurchaseOrder order, CancellationToken cancellationToken)
        {
            calls.Add("save:order");
            Added.Add(order);

            return Task.CompletedTask;
        }
    }

    private sealed class FakeShippingSlips(List<string> calls) : IShippingSlipRepository
    {
        public List<ShippingSlip> Added { get; } = [];

        public Task AddAsync(ShippingSlip slip, CancellationToken cancellationToken)
        {
            calls.Add("save:shipping-slip");
            Added.Add(slip);

            return Task.CompletedTask;
        }
    }

    private sealed class FakePublisher(List<string> calls) : IPublisher
    {
        public Task Publish(object notification, CancellationToken cancellationToken = default)
        {
            calls.Add($"publish:{notification.GetType().Name}");

            return Task.CompletedTask;
        }

        public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
            where TNotification : INotification
        {
            calls.Add($"publish:{notification.GetType().Name}");

            return Task.CompletedTask;
        }
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow()
        {
            return now;
        }
    }
}
