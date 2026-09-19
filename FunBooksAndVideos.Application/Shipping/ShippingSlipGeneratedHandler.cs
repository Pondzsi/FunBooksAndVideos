using FunBooksAndVideos.Domain.Shipping;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FunBooksAndVideos.Application.Shipping;

public sealed class ShippingSlipGeneratedHandler : INotificationHandler<ShippingSlipGenerated>
{
    private readonly ILogger<ShippingSlipGeneratedHandler> _logger;

    public ShippingSlipGeneratedHandler(ILogger<ShippingSlipGeneratedHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(ShippingSlipGenerated notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Shipping slip generated for order {PurchaseOrderId} (customer {CustomerId}).",
            notification.PurchaseOrderId,
            notification.CustomerId);

        return Task.CompletedTask;
    }
}
