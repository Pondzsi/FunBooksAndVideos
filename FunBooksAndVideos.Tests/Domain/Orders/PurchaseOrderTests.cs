using FunBooksAndVideos.Domain.Orders;
using FunBooksAndVideos.Domain.Products;

namespace FunBooksAndVideos.Tests.Domain.Orders;

public class PurchaseOrderTests
{
    // The PDF only gives the total (48.50) for its example order, so these prices are illustrative.
    private static readonly Product FirstAidVideo = new DigitalProduct(1, "Comprehensive First Aid Training", 24.00m, ProductCategory.Video);
    private static readonly Product GirlOnTheTrain = new PhysicalProduct(2, "The Girl on the train", 9.50m, ProductCategory.Book);
    private static readonly Product BookClub = new MembershipProduct(3, "Book Club Membership", 15.00m, [ProductCategory.Book]);

    [Fact]
    public void Has_one_item_line_per_product_in_the_given_order()
    {
        var order = new PurchaseOrder(3344656, 4567890, [FirstAidVideo, GirlOnTheTrain, BookClub]);

        Assert.Equal(new[] { FirstAidVideo, GirlOnTheTrain, BookClub }, order.Items.Select(item => item.Product));
    }

    [Fact]
    public void Total_is_the_sum_of_the_item_prices()
    {
        var order = new PurchaseOrder(3344656, 4567890, [FirstAidVideo, GirlOnTheTrain, BookClub]);

        Assert.Equal(48.50m, order.Total);
    }

    [Fact]
    public void Contains_a_membership_when_a_membership_is_ordered()
    {
        var order = new PurchaseOrder(1, 1, [GirlOnTheTrain, BookClub]);

        Assert.True(order.ContainsMembership);
    }

    [Fact]
    public void Contains_no_membership_when_only_products_are_ordered()
    {
        var order = new PurchaseOrder(1, 1, [FirstAidVideo, GirlOnTheTrain]);

        Assert.False(order.ContainsMembership);
    }

    [Fact]
    public void Contains_a_physical_product_when_one_is_ordered()
    {
        var order = new PurchaseOrder(1, 1, [FirstAidVideo, GirlOnTheTrain]);

        Assert.True(order.ContainsPhysicalProduct);
    }

    [Fact]
    public void Contains_no_physical_product_when_only_videos_and_memberships_are_ordered()
    {
        var order = new PurchaseOrder(1, 1, [FirstAidVideo, BookClub]);

        Assert.False(order.ContainsPhysicalProduct);
    }

    [Fact]
    public void Needs_at_least_one_item()
    {
        Assert.Throws<ArgumentException>(() => new PurchaseOrder(1, 1, []));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Customer_id_must_be_positive(long customerId)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new PurchaseOrder(1, customerId, [GirlOnTheTrain]));
    }
}
