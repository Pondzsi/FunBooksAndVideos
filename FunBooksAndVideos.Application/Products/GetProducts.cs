using FunBooksAndVideos.Domain.Products;

namespace FunBooksAndVideos.Application.Products;

public sealed class GetProducts
{
    private readonly IProductRepository _products;

    public GetProducts(IProductRepository products)
    {
        _products = products;
    }

    public Task<IReadOnlyList<Product>> ExecuteAsync(CancellationToken cancellationToken)
    {
        return _products.GetAllAsync(cancellationToken);
    }
}
