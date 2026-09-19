namespace FunBooksAndVideos.Application.Orders;

public sealed record PlacePurchaseOrderCommand(long CustomerId, IReadOnlyCollection<long> ProductIds);
