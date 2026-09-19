using FunBooksAndVideos.Domain.Products;

namespace FunBooksAndVideos.Tests.Domain.Products;

public class ProductTests
{
    [Fact]
    public void Membership_exposes_the_categories_it_grants()
    {
        var premium = new MembershipProduct(1, "Premium Membership", 25m, [ProductCategory.Video, ProductCategory.Book]);

        Assert.Equal(new[] { ProductCategory.Book, ProductCategory.Video }, premium.GrantedCategories.Order());
    }

    [Fact]
    public void Membership_must_grant_at_least_one_category()
    {
        Assert.Throws<ArgumentException>(() => new MembershipProduct(1, "Empty Membership", 5m, []));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Price_must_be_positive(int price)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new PhysicalProduct(1, "The Girl on the train", price, ProductCategory.Book));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Name_is_required(string name)
    {
        Assert.Throws<ArgumentException>(
            () => new DigitalProduct(1, name, 10m, ProductCategory.Video));
    }
}
