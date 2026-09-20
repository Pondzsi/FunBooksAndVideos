using FunBooksAndVideos.Domain.Products;

namespace FunBooksAndVideos.Application.Products;

public interface IProductRepository
{
    // Allocated before the product is constructed, so a Product always has its ID.
    Task<long> NextIdAsync(CancellationToken cancellationToken);

    Task AddAsync(Product product, CancellationToken cancellationToken);

    Task<Product?> GetByIdAsync(long id, CancellationToken cancellationToken);

    Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken);
}
