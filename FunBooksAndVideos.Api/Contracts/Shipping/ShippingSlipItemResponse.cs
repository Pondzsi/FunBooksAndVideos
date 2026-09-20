using FunBooksAndVideos.Domain.Shipping;

namespace FunBooksAndVideos.Api.Contracts.Shipping;

public sealed record ShippingSlipItemResponse(long ProductId, string ProductName)
{
    public static ShippingSlipItemResponse From(ShippingSlipItem item)
    {
        return new ShippingSlipItemResponse(item.ProductId, item.ProductName);
    }
}
