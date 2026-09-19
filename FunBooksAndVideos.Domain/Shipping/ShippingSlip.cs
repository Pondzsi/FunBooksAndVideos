using FunBooksAndVideos.Domain.Common;
using FunBooksAndVideos.Domain.Orders;
using FunBooksAndVideos.Domain.Products;

namespace FunBooksAndVideos.Domain.Shipping;

public sealed class ShippingSlip : AggregateRoot
{
    private ShippingSlip(long purchaseOrderId, long customerId, IReadOnlyList<PurchaseOrderItem> items, DateTimeOffset generatedAt)
    {
        PurchaseOrderId = purchaseOrderId;
        CustomerId = customerId;
        Items = items;
        GeneratedAt = generatedAt;
    }

    // One slip per order, so the order's ID identifies it.
    public long PurchaseOrderId { get; }

    public long CustomerId { get; }

    public IReadOnlyList<PurchaseOrderItem> Items { get; }

    public DateTimeOffset GeneratedAt { get; }

    public static ShippingSlip Generate(PurchaseOrder order, DateTimeOffset generatedAt)
    {
        ArgumentNullException.ThrowIfNull(order);

        var physicalItems = order.Items.Where(item => item.Product is PhysicalProduct).ToList();

        if (physicalItems.Count == 0)
        {
            throw new ArgumentException("A shipping slip needs an order with at least one physical product.", nameof(order));
        }

        var slip = new ShippingSlip(order.Id, order.CustomerId, physicalItems.AsReadOnly(), generatedAt);
        slip.Raise(new ShippingSlipGenerated(order.Id, order.CustomerId));

        return slip;
    }
}
