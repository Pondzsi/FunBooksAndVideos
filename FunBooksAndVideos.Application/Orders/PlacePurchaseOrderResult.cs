using FunBooksAndVideos.Domain.Orders;
using FunBooksAndVideos.Domain.Shipping;

namespace FunBooksAndVideos.Application.Orders;

public sealed record PlacePurchaseOrderResult(PurchaseOrder Order, ShippingSlip? ShippingSlip);
