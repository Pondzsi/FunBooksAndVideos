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

    public Task<Customer?> GetByIdAsync(long id, CancellationToken cancellationToken)
    {
        return _context.Customers.FirstOrDefaultAsync(customer => customer.Id == id, cancellationToken);
    }
}
