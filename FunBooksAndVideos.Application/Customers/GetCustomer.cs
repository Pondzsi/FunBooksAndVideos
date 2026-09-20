using FunBooksAndVideos.Domain.Customers;

namespace FunBooksAndVideos.Application.Customers;

public sealed class GetCustomer
{
    private readonly ICustomerRepository _customers;

    public GetCustomer(ICustomerRepository customers)
    {
        _customers = customers;
    }

    // Null when there is no such customer.
    public Task<Customer?> ExecuteAsync(long id, CancellationToken cancellationToken)
    {
        return _customers.GetByIdAsync(id, cancellationToken);
    }
}
