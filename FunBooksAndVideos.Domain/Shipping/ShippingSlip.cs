using FunBooksAndVideos.Domain.Common;
using FunBooksAndVideos.Domain.Orders;
using FunBooksAndVideos.Domain.Products;

namespace FunBooksAndVideos.Domain.Shipping;

public sealed class ShippingSlip : AggregateRoot
{
    private readonly List<ShippingSlipItem> _items;

    // For EF Core.
    private ShippingSlip()
    {
        _items = [];
    }

    private ShippingSlip(long purchaseOrderId, long customerId, List<ShippingSlipItem> items, DateTimeOffset generatedAt)
    {
        PurchaseOrderId = purchaseOrderId;
        CustomerId = customerId;
        _items = items;
        GeneratedAt = generatedAt;
    }

    // One slip per order, so the order's ID identifies it.
    public long PurchaseOrderId { get; }

    public long CustomerId { get; }

    public IReadOnlyList<ShippingSlipItem> Items => _items.AsReadOnly();

    public DateTimeOffset GeneratedAt { get; }

    public static ShippingSlip Generate(PurchaseOrder order, DateTimeOffset generatedAt)
    {
        ArgumentNullException.ThrowIfNull(order);

        var items = order.Items
            .Where(item => item.Product is PhysicalProduct)
            .Select(item => new ShippingSlipItem(item.Product.Id, item.Product.Name))
            .ToList();

        if (items.Count == 0)
        {
            throw new ArgumentException("A shipping slip needs an order with at least one physical product.", nameof(order));
        }

        var slip = new ShippingSlip(order.Id, order.CustomerId, items, generatedAt);
        slip.Raise(new ShippingSlipGenerated(order.Id, order.CustomerId));

        return slip;
    }
}
