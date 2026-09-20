using FunBooksAndVideos.Api.Contracts.Products;
using FunBooksAndVideos.Application.Products;
using FunBooksAndVideos.Domain.Orders;
using FunBooksAndVideos.Domain.Products;

namespace FunBooksAndVideos.Api.Contracts.PurchaseOrders;

// Price is what the product cost when the order was placed. Categories says whether it is a book or a video, or what a membership covers.
public sealed record PurchaseOrderItemResponse(long ProductId, string ProductName, ProductKind Kind, IReadOnlyList<ProductCategory> Categories, decimal Price)
{
    public static PurchaseOrderItemResponse From(PurchaseOrderItem item)
    {
        return new PurchaseOrderItemResponse(item.Product.Id, item.Product.Name, item.Product.ToKind(), item.Product.ToCategories(), item.Price);
    }
}
