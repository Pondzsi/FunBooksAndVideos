using FunBooksAndVideos.Domain.Products;

namespace FunBooksAndVideos.Application.Products;

public sealed class GetProduct
{
    private readonly IProductRepository _products;

    public GetProduct(IProductRepository products)
    {
        _products = products;
    }

    // Null when there is no such product.
    public Task<Product?> ExecuteAsync(long id, CancellationToken cancellationToken)
    {
        return _products.GetByIdAsync(id, cancellationToken);
    }
}
