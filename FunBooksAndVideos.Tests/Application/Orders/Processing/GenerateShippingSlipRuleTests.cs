using FunBooksAndVideos.Application.Orders.Processing;
using FunBooksAndVideos.Domain.Orders;
using FunBooksAndVideos.Domain.Products;

namespace FunBooksAndVideos.Tests.Application.Orders.Processing;

public class GenerateShippingSlipRuleTests
{
    private readonly GenerateShippingSlipRule _rule = new();

    private static PurchaseOrderProcessingContext ContextFor(params Product[] products)
    {
        var order = new PurchaseOrder(3344656, 4567890, products);

        return new PurchaseOrderProcessingContext(order, TestData.NewCustomer(), TestData.Now);
    }

    [Fact]
    public void Generates_a_shipping_slip_when_the_order_contains_a_physical_product()
    {
        var context = ContextFor(TestData.FirstAidVideo, TestData.GirlOnTheTrain);

        _rule.Apply(context);

        Assert.NotNull(context.ShippingSlip);
        Assert.Equal(3344656, context.ShippingSlip.PurchaseOrderId);
        Assert.Equal(TestData.Now, context.ShippingSlip.GeneratedAt);
    }

    [Fact]
    public void Generates_no_shipping_slip_for_videos_and_memberships_only()
    {
        var context = ContextFor(TestData.FirstAidVideo, TestData.BookClub);

        _rule.Apply(context);

        Assert.Null(context.ShippingSlip);
    }
}
