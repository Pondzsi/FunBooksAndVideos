using FunBooksAndVideos.Application.Shipping;
using FunBooksAndVideos.Domain.Shipping;

namespace FunBooksAndVideos.Infrastructure.Persistence.Repositories;

internal sealed class ShippingSlipRepository : IShippingSlipRepository
{
    private readonly AppDbContext _context;

    public ShippingSlipRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task AddAsync(ShippingSlip slip, CancellationToken cancellationToken)
    {
        _context.ShippingSlips.Add(slip);

        return Task.CompletedTask;
    }
}
