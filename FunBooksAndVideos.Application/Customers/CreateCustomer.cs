using FunBooksAndVideos.Application.Common;
using FunBooksAndVideos.Domain.Customers;

namespace FunBooksAndVideos.Application.Customers;

public sealed class CreateCustomer
{
    private readonly ICustomerRepository _customers;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCustomer(ICustomerRepository customers, IUnitOfWork unitOfWork)
    {
        _customers = customers;
        _unitOfWork = unitOfWork;
    }

    public async Task<Customer> ExecuteAsync(string name, CancellationToken cancellationToken)
    {
        var customer = new Customer(await _customers.NextIdAsync(cancellationToken), name);

        await _customers.AddAsync(customer, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return customer;
    }
}
