using FunBooksAndVideos.Api.Contracts.Products;
using FunBooksAndVideos.Domain.Orders;

namespace FunBooksAndVideos.Api.Contracts.PurchaseOrders;

// Price is what the product cost when the order was placed.
public sealed record PurchaseOrderItemResponse(long ProductId, string ProductName, ProductKind Kind, decimal Price)
{
    public static PurchaseOrderItemResponse From(PurchaseOrderItem item)
    {
        return new PurchaseOrderItemResponse(item.Product.Id, item.Product.Name, item.Product.ToKind(), item.Price);
    }
}
