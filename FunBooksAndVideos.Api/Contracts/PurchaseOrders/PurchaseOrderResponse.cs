using FunBooksAndVideos.Domain.Orders;

namespace FunBooksAndVideos.Api.Contracts.PurchaseOrders;

public sealed record PurchaseOrderResponse(long Id, long CustomerId, decimal Total, IReadOnlyList<PurchaseOrderItemResponse> Items)
{
    public static PurchaseOrderResponse From(PurchaseOrder order)
    {
        return new PurchaseOrderResponse(
            order.Id,
            order.CustomerId,
            order.Total,
            order.Items.Select(PurchaseOrderItemResponse.From).ToList());
    }
}
