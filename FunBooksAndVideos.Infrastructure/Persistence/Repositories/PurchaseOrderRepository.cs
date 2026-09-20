using FunBooksAndVideos.Application.Orders;
using FunBooksAndVideos.Domain.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace FunBooksAndVideos.Infrastructure.Persistence.Repositories;

internal sealed class PurchaseOrderRepository : IPurchaseOrderRepository
{
    private readonly AppDbContext _context;

    public PurchaseOrderRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<long> NextIdAsync(CancellationToken cancellationToken)
    {
        await _context.Database.OpenConnectionAsync(cancellationToken);

        try
        {
            await using var command = _context.Database.GetDbConnection().CreateCommand();
            command.Transaction = _context.Database.CurrentTransaction?.GetDbTransaction();
            command.CommandText = $"SELECT NEXT VALUE FOR [{AppDbContext.PurchaseOrderIdSequence}]";

            return Convert.ToInt64(await command.ExecuteScalarAsync(cancellationToken));
        }
        finally
        {
            await _context.Database.CloseConnectionAsync();
        }
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
}
