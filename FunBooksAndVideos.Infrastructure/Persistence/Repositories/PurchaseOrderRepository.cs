using FunBooksAndVideos.Application.Orders;
using FunBooksAndVideos.Domain.Orders;
using Microsoft.EntityFrameworkCore;

namespace FunBooksAndVideos.Infrastructure.Persistence.Repositories;

internal sealed class PurchaseOrderRepository : IPurchaseOrderRepository
{
    private readonly AppDbContext _context;

    public PurchaseOrderRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<long> NextIdAsync(CancellationToken cancellationToken)
    {
        return SequenceIds.NextAsync(_context, AppDbContext.PurchaseOrderIdSequence, cancellationToken);
    }

    public Task AddAsync(PurchaseOrder order, CancellationToken cancellationToken)
    {
        _context.PurchaseOrders.Add(order);

        return Task.CompletedTask;
    }

    public Task<PurchaseOrder?> GetByIdAsync(long id, CancellationToken cancellationToken)
    {
        return _context.PurchaseOrders
            .AsNoTracking()
            .Include(order => order.Items)
            .ThenInclude(item => item.Product)
            .FirstOrDefaultAsync(order => order.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<PurchaseOrder>> GetAllAsync(long? customerId, CancellationToken cancellationToken)
    {
        var orders = _context.PurchaseOrders.AsNoTracking();

        if (customerId is not null)
        {
            orders = orders.Where(order => order.CustomerId == customerId);
        }

        return await orders
            .Include(order => order.Items)
            .ThenInclude(item => item.Product)
            .OrderBy(order => order.Id)
            .ToListAsync(cancellationToken);
    }
}
