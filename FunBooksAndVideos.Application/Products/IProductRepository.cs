using FunBooksAndVideos.Domain.Products;

namespace FunBooksAndVideos.Application.Products;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(long id, CancellationToken cancellationToken);

    Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken);
}
