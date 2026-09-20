using FunBooksAndVideos.Domain.Orders;

namespace FunBooksAndVideos.Application.Orders;

public sealed class GetPurchaseOrders
{
    private readonly IPurchaseOrderRepository _orders;

    public GetPurchaseOrders(IPurchaseOrderRepository orders)
    {
        _orders = orders;
    }

    public Task<IReadOnlyList<PurchaseOrder>> ExecuteAsync(long? customerId, CancellationToken cancellationToken)
    {
        return _orders.GetAllAsync(customerId, cancellationToken);
    }
}
