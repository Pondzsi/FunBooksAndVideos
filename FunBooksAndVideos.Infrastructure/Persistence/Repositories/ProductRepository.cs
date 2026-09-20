using FunBooksAndVideos.Application.Products;
using FunBooksAndVideos.Domain.Products;
using Microsoft.EntityFrameworkCore;

namespace FunBooksAndVideos.Infrastructure.Persistence.Repositories;

internal sealed class ProductRepository : IProductRepository
{
    private readonly AppDbContext _context;

    public ProductRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<Product?> GetByIdAsync(long id, CancellationToken cancellationToken)
    {
        return _context.Products.FirstOrDefaultAsync(product => product.Id == id, cancellationToken);
    }
}
