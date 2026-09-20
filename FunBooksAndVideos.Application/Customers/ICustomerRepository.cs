using FunBooksAndVideos.Domain.Customers;

namespace FunBooksAndVideos.Application.Customers;

public interface ICustomerRepository
{
    // Allocated before the customer is constructed, so a Customer always has its ID.
    Task<long> NextIdAsync(CancellationToken cancellationToken);

    Task AddAsync(Customer customer, CancellationToken cancellationToken);

    // Changes to a loaded customer are persisted by the unit of work.
    Task<Customer?> GetByIdAsync(long id, CancellationToken cancellationToken);

    Task<IReadOnlyList<Customer>> GetAllAsync(CancellationToken cancellationToken);
}
