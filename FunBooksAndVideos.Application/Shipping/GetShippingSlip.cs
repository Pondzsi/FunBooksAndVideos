using FunBooksAndVideos.Domain.Shipping;

namespace FunBooksAndVideos.Application.Shipping;

public sealed class GetShippingSlip
{
    private readonly IShippingSlipRepository _shippingSlips;

    public GetShippingSlip(IShippingSlipRepository shippingSlips)
    {
        _shippingSlips = shippingSlips;
    }

    // Null when the order has no slip, either because it needed none or because there is no such order.
    public Task<ShippingSlip?> ExecuteAsync(long purchaseOrderId, CancellationToken cancellationToken)
    {
        return _shippingSlips.GetByPurchaseOrderIdAsync(purchaseOrderId, cancellationToken);
    }
}
