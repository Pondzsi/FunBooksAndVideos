using System.ComponentModel.DataAnnotations;

namespace FunBooksAndVideos.Api.Contracts.PurchaseOrders;

// Prices are never sent: they always come from the catalog.
public sealed record PlacePurchaseOrderRequest(
    [Range(1, long.MaxValue, ErrorMessage = "CustomerId must be a positive number.")] long CustomerId,
    [Required, MinLength(1, ErrorMessage = "ProductIds must contain at least one product ID.")] IReadOnlyList<long> ProductIds);
