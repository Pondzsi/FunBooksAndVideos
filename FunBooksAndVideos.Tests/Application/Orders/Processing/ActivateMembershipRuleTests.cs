using FunBooksAndVideos.Application.Orders.Processing;
using FunBooksAndVideos.Domain.Orders;
using FunBooksAndVideos.Domain.Products;

namespace FunBooksAndVideos.Tests.Application.Orders.Processing;

public class ActivateMembershipRuleTests
{
    private readonly ActivateMembershipRule _rule = new();

    private static PurchaseOrderProcessingContext ContextFor(params Product[] products)
    {
        var order = new PurchaseOrder(3344656, 4567890, products);

        return new PurchaseOrderProcessingContext(order, TestData.NewCustomer(), TestData.Now);
    }

    [Fact]
    public void Activates_the_membership_in_the_order_on_the_customer_account()
    {
        var context = ContextFor(TestData.GirlOnTheTrain, TestData.BookClub);

        _rule.Apply(context);

        var membership = Assert.Single(context.Customer.Memberships);
        Assert.Equal(TestData.BookClub.Id, membership.ProductId);
        Assert.True(context.Customer.HasAccessTo(ProductCategory.Book));
        Assert.False(context.Customer.HasAccessTo(ProductCategory.Video));
    }

    [Fact]
    public void Activates_immediately_at_the_processing_time()
    {
        var context = ContextFor(TestData.BookClub);

        _rule.Apply(context);

        Assert.Equal(TestData.Now, Assert.Single(context.Customer.Memberships).ActivatedAt);
    }

    [Fact]
    public void Premium_gives_access_to_both_categories()
    {
        var context = ContextFor(TestData.Premium);

        _rule.Apply(context);

        Assert.Single(context.Customer.Memberships);
        Assert.True(context.Customer.HasAccessTo(ProductCategory.Book));
        Assert.True(context.Customer.HasAccessTo(ProductCategory.Video));
    }

    [Fact]
    public void Does_nothing_when_the_order_has_no_membership()
    {
        var context = ContextFor(TestData.FirstAidVideo, TestData.GirlOnTheTrain);

        _rule.Apply(context);

        Assert.Empty(context.Customer.Memberships);
    }
}
