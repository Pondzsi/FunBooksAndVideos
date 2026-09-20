using FunBooksAndVideos.Domain.Customers;

namespace FunBooksAndVideos.Application.Customers;

public sealed class GetCustomers
{
    private readonly ICustomerRepository _customers;

    public GetCustomers(ICustomerRepository customers)
    {
        _customers = customers;
    }

    public Task<IReadOnlyList<Customer>> ExecuteAsync(CancellationToken cancellationToken)
    {
        return _customers.GetAllAsync(cancellationToken);
    }
}
