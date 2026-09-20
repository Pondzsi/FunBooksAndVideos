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

    public Task<long> NextIdAsync(CancellationToken cancellationToken)
    {
        return SequenceIds.NextAsync(_context, AppDbContext.ProductIdSequence, cancellationToken);
    }

    public Task AddAsync(Product product, CancellationToken cancellationToken)
    {
        _context.Products.Add(product);

        return Task.CompletedTask;
    }

    public Task<Product?> GetByIdAsync(long id, CancellationToken cancellationToken)
    {
        return _context.Products.FirstOrDefaultAsync(product => product.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _context.Products.AsNoTracking().OrderBy(product => product.Id).ToListAsync(cancellationToken);
    }
}
