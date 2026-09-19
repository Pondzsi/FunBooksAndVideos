using FunBooksAndVideos.Application.Common;

namespace FunBooksAndVideos.Application.Products;

public sealed class ProductNotFoundException : NotFoundException
{
    public ProductNotFoundException(long productId)
        : base($"Product {productId} was not found.")
    {
        ProductId = productId;
    }

    public long ProductId { get; }
}
