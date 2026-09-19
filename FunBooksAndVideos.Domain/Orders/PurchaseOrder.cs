using FunBooksAndVideos.Domain.Products;

namespace FunBooksAndVideos.Domain.Orders;

public sealed class PurchaseOrder
{
    public PurchaseOrder(long id, long customerId, IEnumerable<Product> products)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(customerId);
        ArgumentNullException.ThrowIfNull(products);

        Items = products.Select(product => new PurchaseOrderItem(product)).ToList().AsReadOnly();

        if (Items.Count == 0)
        {
            throw new ArgumentException("A purchase order needs at least one item.", nameof(products));
        }

        Id = id;
        CustomerId = customerId;
    }

    public long Id { get; }

    public long CustomerId { get; }

    public IReadOnlyList<PurchaseOrderItem> Items { get; }

    public decimal Total => Items.Sum(item => item.Price);

    public bool ContainsMembership => Items.Any(item => item.Product is MembershipProduct);

    public bool ContainsPhysicalProduct => Items.Any(item => item.Product is PhysicalProduct);
}
