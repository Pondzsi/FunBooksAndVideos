using FunBooksAndVideos.Application.Common;

namespace FunBooksAndVideos.Application.Customers;

public sealed class CustomerNotFoundException : NotFoundException
{
    public CustomerNotFoundException(long customerId)
        : base($"Customer {customerId} was not found.")
    {
        CustomerId = customerId;
    }

    public long CustomerId { get; }
}
