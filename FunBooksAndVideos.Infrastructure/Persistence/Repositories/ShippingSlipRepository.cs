using FunBooksAndVideos.Application.Shipping;
using FunBooksAndVideos.Domain.Shipping;
using Microsoft.EntityFrameworkCore;

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

    public Task<ShippingSlip?> GetByPurchaseOrderIdAsync(long purchaseOrderId, CancellationToken cancellationToken)
    {
        return _context.ShippingSlips
            .AsNoTracking()
            .FirstOrDefaultAsync(slip => slip.PurchaseOrderId == purchaseOrderId, cancellationToken);
    }
}
