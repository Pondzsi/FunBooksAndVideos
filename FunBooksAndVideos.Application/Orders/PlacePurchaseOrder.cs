using FunBooksAndVideos.Application.Common;
using FunBooksAndVideos.Application.Customers;
using FunBooksAndVideos.Application.Orders.Processing;
using FunBooksAndVideos.Application.Products;
using FunBooksAndVideos.Application.Shipping;
using FunBooksAndVideos.Domain.Common;
using FunBooksAndVideos.Domain.Customers;
using FunBooksAndVideos.Domain.Orders;
using FunBooksAndVideos.Domain.Products;
using FunBooksAndVideos.Domain.Shipping;
using MediatR;

namespace FunBooksAndVideos.Application.Orders;

public sealed class PlacePurchaseOrder
{
    private readonly ICustomerRepository _customers;
    private readonly IProductRepository _products;
    private readonly IPurchaseOrderRepository _orders;
    private readonly IShippingSlipRepository _shippingSlips;
    private readonly IUnitOfWork _unitOfWork;
    private readonly PurchaseOrderProcessor _processor;
    private readonly IPublisher _publisher;
    private readonly TimeProvider _timeProvider;

    public PlacePurchaseOrder(
        ICustomerRepository customers,
        IProductRepository products,
        IPurchaseOrderRepository orders,
        IShippingSlipRepository shippingSlips,
        IUnitOfWork unitOfWork,
        PurchaseOrderProcessor processor,
        IPublisher publisher,
        TimeProvider timeProvider)
    {
        _customers = customers;
        _products = products;
        _orders = orders;
        _shippingSlips = shippingSlips;
        _unitOfWork = unitOfWork;
        _processor = processor;
        _publisher = publisher;
        _timeProvider = timeProvider;
    }

    public async Task<PlacePurchaseOrderResult> ExecuteAsync(PlacePurchaseOrderCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var customer = await _customers.GetByIdAsync(command.CustomerId, cancellationToken)
            ?? throw new CustomerNotFoundException(command.CustomerId);

        var products = new List<Product>();

        foreach (var productId in command.ProductIds)
        {
            var product = await _products.GetByIdAsync(productId, cancellationToken)
                ?? throw new ProductNotFoundException(productId);

            products.Add(product);
        }

        var orderId = await _orders.NextIdAsync(cancellationToken);
        var order = new PurchaseOrder(orderId, customer.Id, products);

        var context = new PurchaseOrderProcessingContext(order, customer, _timeProvider.GetUtcNow());
        _processor.Process(context);

        await _orders.AddAsync(order, cancellationToken);

        if (context.ShippingSlip is not null)
        {
            await _shippingSlips.AddAsync(context.ShippingSlip, cancellationToken);
        }

        // The order, the customer's new memberships and the slip are saved together, or not at all.
        await _unitOfWork.CommitAsync(cancellationToken);

        await PublishDomainEventsAsync(customer, context.ShippingSlip, cancellationToken);

        return new PlacePurchaseOrderResult(order, context.ShippingSlip);
    }

    // Published only after the commit, so handlers never see state that was not persisted.
    private async Task PublishDomainEventsAsync(Customer customer, ShippingSlip? shippingSlip, CancellationToken cancellationToken)
    {
        List<AggregateRoot> aggregates = [customer];

        if (shippingSlip is not null)
        {
            aggregates.Add(shippingSlip);
        }

        var domainEvents = aggregates.SelectMany(aggregate => aggregate.DomainEvents).ToList();

        foreach (var aggregate in aggregates)
        {
            aggregate.ClearDomainEvents();
        }

        foreach (var domainEvent in domainEvents)
        {
            await _publisher.Publish(domainEvent, cancellationToken);
        }
    }
}
