using FunBooksAndVideos.Domain.Customers;

namespace FunBooksAndVideos.Application.Customers;

public interface ICustomerRepository
{
    // Changes to a loaded customer are persisted by the unit of work.
    Task<Customer?> GetByIdAsync(long id, CancellationToken cancellationToken);
}
