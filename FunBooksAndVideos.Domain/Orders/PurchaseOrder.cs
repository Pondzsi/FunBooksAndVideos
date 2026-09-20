using FunBooksAndVideos.Domain.Products;

namespace FunBooksAndVideos.Domain.Orders;

public sealed class PurchaseOrder
{
    private readonly List<PurchaseOrderItem> _items;

    // For EF Core.
    private PurchaseOrder()
    {
        _items = [];
    }

    public PurchaseOrder(long id, long customerId, IEnumerable<Product> products)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(customerId);
        ArgumentNullException.ThrowIfNull(products);

        _items = products.Select(product => new PurchaseOrderItem(product)).ToList();

        if (_items.Count == 0)
        {
            throw new ArgumentException("A purchase order needs at least one item.", nameof(products));
        }

        Id = id;
        CustomerId = customerId;
    }

    public long Id { get; }

    public long CustomerId { get; }

    public IReadOnlyList<PurchaseOrderItem> Items => _items.AsReadOnly();

    public decimal Total => _items.Sum(item => item.Price);

    public bool ContainsMembership => _items.Any(item => item.Product is MembershipProduct);

    public bool ContainsPhysicalProduct => _items.Any(item => item.Product is PhysicalProduct);
}
