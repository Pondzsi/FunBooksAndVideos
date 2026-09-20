using FunBooksAndVideos.Domain.Products;

namespace FunBooksAndVideos.Domain.Orders;

public sealed class PurchaseOrderItem
{
    public PurchaseOrderItem(Product product)
    {
        ArgumentNullException.ThrowIfNull(product);

        Product = product;
        Price = product.Price;
    }

    // For EF Core.
    private PurchaseOrderItem()
    {
        Product = null!;
    }

    public Product Product { get; }

    // Price at the time of the order, so later catalog changes don't rewrite it.
    public decimal Price { get; }
}
