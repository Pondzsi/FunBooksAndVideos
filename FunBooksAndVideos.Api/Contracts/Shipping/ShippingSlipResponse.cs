using FunBooksAndVideos.Domain.Shipping;

namespace FunBooksAndVideos.Api.Contracts.Shipping;

public sealed record ShippingSlipResponse(
    long PurchaseOrderId,
    long CustomerId,
    DateTimeOffset GeneratedAt,
    IReadOnlyList<ShippingSlipItemResponse> Items)
{
    public static ShippingSlipResponse From(ShippingSlip slip)
    {
        return new ShippingSlipResponse(
            slip.PurchaseOrderId,
            slip.CustomerId,
            slip.GeneratedAt,
            slip.Items.Select(ShippingSlipItemResponse.From).ToList());
    }
}
