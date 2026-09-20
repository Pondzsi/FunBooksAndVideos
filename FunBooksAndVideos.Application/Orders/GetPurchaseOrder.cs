using FunBooksAndVideos.Domain.Orders;

namespace FunBooksAndVideos.Application.Orders;

public sealed class GetPurchaseOrder
{
    private readonly IPurchaseOrderRepository _orders;

    public GetPurchaseOrder(IPurchaseOrderRepository orders)
    {
        _orders = orders;
    }

    // Null when there is no such order.
    public Task<PurchaseOrder?> ExecuteAsync(long id, CancellationToken cancellationToken)
    {
        return _orders.GetByIdAsync(id, cancellationToken);
    }
}
