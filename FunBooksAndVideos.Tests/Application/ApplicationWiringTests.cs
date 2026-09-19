using FunBooksAndVideos.Application;
using FunBooksAndVideos.Application.Orders.Processing;
using FunBooksAndVideos.Domain.Customers;
using FunBooksAndVideos.Domain.Shipping;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FunBooksAndVideos.Tests.Application;

public class ApplicationWiringTests
{
    private static ServiceProvider BuildProvider(LogSink sink)
    {
        var services = new ServiceCollection();
        services.AddSingleton(sink);
        services.AddSingleton(typeof(ILogger<>), typeof(ListLogger<>));
        services.AddApplication();

        return services.BuildServiceProvider();
    }

    [Fact]
    public void Registers_the_business_rules_in_the_order_they_run()
    {
        using var provider = BuildProvider(new LogSink());
        using var scope = provider.CreateScope();

        var rules = scope.ServiceProvider.GetServices<IPurchaseOrderRule>().Select(rule => rule.GetType());

        Assert.Equal(new[] { typeof(ActivateMembershipRule), typeof(GenerateShippingSlipRule) }, rules);
    }

    [Fact]
    public void Resolves_a_processor_that_applies_both_rules()
    {
        using var provider = BuildProvider(new LogSink());
        using var scope = provider.CreateScope();
        var processor = scope.ServiceProvider.GetRequiredService<PurchaseOrderProcessor>();
        var context = new PurchaseOrderProcessingContext(TestData.PdfExampleOrder(), TestData.NewCustomer(), TestData.Now);

        processor.Process(context);

        Assert.NotEmpty(context.Customer.Memberships);
        Assert.NotNull(context.ShippingSlip);
    }

    [Fact]
    public async Task Delivers_MembershipActivated_to_its_handler()
    {
        var sink = new LogSink();
        using var provider = BuildProvider(sink);
        using var scope = provider.CreateScope();

        await scope.ServiceProvider.GetRequiredService<IPublisher>().Publish(new MembershipActivated(4567890, 3));

        Assert.Contains("Membership 3 activated for customer 4567890.", sink.Messages);
    }

    [Fact]
    public async Task Delivers_ShippingSlipGenerated_to_its_handler()
    {
        var sink = new LogSink();
        using var provider = BuildProvider(sink);
        using var scope = provider.CreateScope();

        await scope.ServiceProvider.GetRequiredService<IPublisher>().Publish(new ShippingSlipGenerated(3344656, 4567890));

        Assert.Contains("Shipping slip generated for order 3344656 (customer 4567890).", sink.Messages);
    }
}

internal sealed class LogSink
{
    public List<string> Messages { get; } = [];
}

internal sealed class ListLogger<T>(LogSink sink) : ILogger<T>
{
    public IDisposable? BeginScope<TState>(TState state)
        where TState : notnull
    {
        return null;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        sink.Messages.Add(formatter(state, exception));
    }
}
