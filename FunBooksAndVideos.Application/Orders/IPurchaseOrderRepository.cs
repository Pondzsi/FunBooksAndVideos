using FunBooksAndVideos.Domain.Orders;

namespace FunBooksAndVideos.Application.Orders;

public interface IPurchaseOrderRepository
{
    // Allocated before the order is constructed, so a PurchaseOrder always has its ID.
    Task<long> NextIdAsync(CancellationToken cancellationToken);

    Task AddAsync(PurchaseOrder order, CancellationToken cancellationToken);

    Task<PurchaseOrder?> GetByIdAsync(long id, CancellationToken cancellationToken);
}
