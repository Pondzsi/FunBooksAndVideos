using FunBooksAndVideos.Domain.Orders;
using FunBooksAndVideos.Domain.Shipping;

namespace FunBooksAndVideos.Tests.Domain.Shipping;

public class ShippingSlipTests
{
    [Fact]
    public void Lists_only_the_physical_items_of_the_order()
    {
        var slip = ShippingSlip.Generate(TestData.PdfExampleOrder(), TestData.Now);

        var item = Assert.Single(slip.Items);
        Assert.Equal(new ShippingSlipItem(TestData.GirlOnTheTrain.Id, TestData.GirlOnTheTrain.Name), item);
    }

    [Fact]
    public void Records_the_order_the_customer_and_when_it_was_generated()
    {
        var slip = ShippingSlip.Generate(TestData.PdfExampleOrder(), TestData.Now);

        Assert.Equal(3344656, slip.PurchaseOrderId);
        Assert.Equal(4567890, slip.CustomerId);
        Assert.Equal(TestData.Now, slip.GeneratedAt);
    }

    [Fact]
    public void Raises_ShippingSlipGenerated()
    {
        var slip = ShippingSlip.Generate(TestData.PdfExampleOrder(), TestData.Now);

        var domainEvent = Assert.Single(slip.DomainEvents);
        Assert.Equal(new ShippingSlipGenerated(3344656, 4567890), domainEvent);
    }

    [Fact]
    public void Needs_an_order_with_a_physical_product()
    {
        var order = new PurchaseOrder(1, 1, [TestData.FirstAidVideo, TestData.BookClub]);

        Assert.Throws<ArgumentException>(() => ShippingSlip.Generate(order, TestData.Now));
    }

    [Fact]
    public void Needs_an_order()
    {
        Assert.Throws<ArgumentNullException>(() => ShippingSlip.Generate(null!, TestData.Now));
    }
}
