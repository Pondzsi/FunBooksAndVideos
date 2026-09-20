using FunBooksAndVideos.Application.Customers;
using FunBooksAndVideos.Domain.Customers;
using Microsoft.EntityFrameworkCore;

namespace FunBooksAndVideos.Infrastructure.Persistence.Repositories;

internal sealed class CustomerRepository : ICustomerRepository
{
    private readonly AppDbContext _context;

    public CustomerRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<long> NextIdAsync(CancellationToken cancellationToken)
    {
        return SequenceIds.NextAsync(_context, AppDbContext.CustomerIdSequence, cancellationToken);
    }

    public Task AddAsync(Customer customer, CancellationToken cancellationToken)
    {
        _context.Customers.Add(customer);

        return Task.CompletedTask;
    }

    public Task<Customer?> GetByIdAsync(long id, CancellationToken cancellationToken)
    {
        return _context.Customers.FirstOrDefaultAsync(customer => customer.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Customer>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _context.Customers.AsNoTracking().OrderBy(customer => customer.Id).ToListAsync(cancellationToken);
    }
}
