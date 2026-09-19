using FunBooksAndVideos.Domain.Shipping;

namespace FunBooksAndVideos.Application.Orders.Processing;

// BR2: an order containing a physical product gets a shipping slip.
public sealed class GenerateShippingSlipRule : IPurchaseOrderRule
{
    public void Apply(PurchaseOrderProcessingContext context)
    {
        if (!context.Order.ContainsPhysicalProduct)
        {
            return;
        }

        context.ShippingSlip = ShippingSlip.Generate(context.Order, context.ProcessedAt);
    }
}
