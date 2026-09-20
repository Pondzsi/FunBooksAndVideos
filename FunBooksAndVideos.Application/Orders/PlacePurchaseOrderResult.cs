using FunBooksAndVideos.Domain.Customers;
using FunBooksAndVideos.Domain.Orders;
using FunBooksAndVideos.Domain.Shipping;

namespace FunBooksAndVideos.Application.Orders;

// ActivatedMemberships are the ones this order activated: a membership the customer already held is not listed.
public sealed record PlacePurchaseOrderResult(PurchaseOrder Order, ShippingSlip? ShippingSlip, IReadOnlyList<Membership> ActivatedMemberships);
