using FunBooksAndVideos.Domain.Customers;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FunBooksAndVideos.Application.Customers;

public sealed class MembershipActivatedHandler : INotificationHandler<MembershipActivated>
{
    private readonly ILogger<MembershipActivatedHandler> _logger;

    public MembershipActivatedHandler(ILogger<MembershipActivatedHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(MembershipActivated notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Membership {MembershipProductId} activated for customer {CustomerId}.",
            notification.MembershipProductId,
            notification.CustomerId);

        return Task.CompletedTask;
    }
}
