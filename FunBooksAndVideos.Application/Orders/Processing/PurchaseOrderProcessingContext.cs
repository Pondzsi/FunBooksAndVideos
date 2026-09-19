using FunBooksAndVideos.Domain.Customers;
using FunBooksAndVideos.Domain.Orders;
using FunBooksAndVideos.Domain.Shipping;

namespace FunBooksAndVideos.Application.Orders.Processing;

public sealed class PurchaseOrderProcessingContext
{
    public PurchaseOrderProcessingContext(PurchaseOrder order, Customer customer, DateTimeOffset processedAt)
    {
        ArgumentNullException.ThrowIfNull(order);
        ArgumentNullException.ThrowIfNull(customer);

        if (customer.Id != order.CustomerId)
        {
            throw new ArgumentException("The customer must be the one who placed the order.", nameof(customer));
        }

        Order = order;
        Customer = customer;
        ProcessedAt = processedAt;
    }

    public PurchaseOrder Order { get; }

    public Customer Customer { get; }

    public DateTimeOffset ProcessedAt { get; }

    public ShippingSlip? ShippingSlip { get; set; }
}
