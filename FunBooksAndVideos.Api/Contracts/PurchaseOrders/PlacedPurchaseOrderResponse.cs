using FunBooksAndVideos.Api.Contracts.Customers;
using FunBooksAndVideos.Application.Orders;

namespace FunBooksAndVideos.Api.Contracts.PurchaseOrders;

// What placing an order did: the order itself, the memberships it activated (BR1) and whether a shipping slip was generated (BR2).
public sealed record PlacedPurchaseOrderResponse(
    long Id,
    long CustomerId,
    decimal Total,
    IReadOnlyList<PurchaseOrderItemResponse> Items,
    IReadOnlyList<MembershipResponse> ActivatedMemberships,
    bool ShippingSlipGenerated)
{
    public static PlacedPurchaseOrderResponse From(PlacePurchaseOrderResult result)
    {
        return new PlacedPurchaseOrderResponse(
            result.Order.Id,
            result.Order.CustomerId,
            result.Order.Total,
            result.Order.Items.Select(PurchaseOrderItemResponse.From).ToList(),
            result.ActivatedMemberships.Select(MembershipResponse.From).ToList(),
            result.ShippingSlip is not null);
    }
}
