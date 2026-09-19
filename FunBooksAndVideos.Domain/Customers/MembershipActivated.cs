using FunBooksAndVideos.Domain.Common;

namespace FunBooksAndVideos.Domain.Customers;

public sealed record MembershipActivated(long CustomerId, long MembershipProductId) : IDomainEvent;
