using FunBooksAndVideos.Domain.Products;

namespace FunBooksAndVideos.Application.Orders.Processing;

// BR1: memberships in the order are activated on the customer account immediately.
public sealed class ActivateMembershipRule : IPurchaseOrderRule
{
    public void Apply(PurchaseOrderProcessingContext context)
    {
        var memberships = context.Order.Items
            .Select(item => item.Product)
            .OfType<MembershipProduct>();

        foreach (var membership in memberships)
        {
            context.Customer.ActivateMembership(membership, context.ProcessedAt);
        }
    }
}
