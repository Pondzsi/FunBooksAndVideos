using FunBooksAndVideos.Domain.Common;

namespace FunBooksAndVideos.Domain.Shipping;

public sealed record ShippingSlipGenerated(long PurchaseOrderId, long CustomerId) : IDomainEvent;
