using FunBooksAndVideos.Domain.Shipping;

namespace FunBooksAndVideos.Application.Shipping;

public interface IShippingSlipRepository
{
    Task AddAsync(ShippingSlip slip, CancellationToken cancellationToken);
}
