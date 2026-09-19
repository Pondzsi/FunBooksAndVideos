using FunBooksAndVideos.Domain.Customers;

namespace FunBooksAndVideos.Application.Customers;

public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(long id, CancellationToken cancellationToken);

    Task SaveAsync(Customer customer, CancellationToken cancellationToken);
}
