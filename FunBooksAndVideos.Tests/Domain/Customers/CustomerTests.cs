using FunBooksAndVideos.Domain.Customers;
using FunBooksAndVideos.Domain.Products;

namespace FunBooksAndVideos.Tests.Domain.Customers;

public class CustomerTests
{
    private static readonly MembershipProduct BookClub = new(1, "Book Club Membership", 15m, [ProductCategory.Book]);
    private static readonly MembershipProduct VideoClub = new(2, "Video Club Membership", 15m, [ProductCategory.Video]);
    private static readonly MembershipProduct Premium = new(3, "Premium Membership", 25m, [ProductCategory.Book, ProductCategory.Video]);

    private static readonly DateTimeOffset Now = new(2026, 9, 19, 12, 0, 0, TimeSpan.Zero);

    private static Customer NewCustomer() => new(4567890, "Test Customer");

    [Fact]
    public void Has_no_memberships_and_no_access_when_created()
    {
        var customer = NewCustomer();

        Assert.Empty(customer.Memberships);
        Assert.False(customer.HasAccessTo(ProductCategory.Book));
        Assert.False(customer.HasAccessTo(ProductCategory.Video));
    }

    [Fact]
    public void Activating_a_membership_gives_access_only_to_the_categories_it_grants()
    {
        var customer = NewCustomer();

        customer.ActivateMembership(BookClub, Now);

        Assert.True(customer.HasAccessTo(ProductCategory.Book));
        Assert.False(customer.HasAccessTo(ProductCategory.Video));
    }

    [Fact]
    public void Premium_is_one_membership_that_gives_access_to_both_categories()
    {
        var customer = NewCustomer();

        customer.ActivateMembership(Premium, Now);

        Assert.Single(customer.Memberships);
        Assert.True(customer.HasAccessTo(ProductCategory.Book));
        Assert.True(customer.HasAccessTo(ProductCategory.Video));
    }

    [Fact]
    public void Access_is_the_union_of_all_memberships()
    {
        var customer = NewCustomer();

        customer.ActivateMembership(BookClub, Now);
        customer.ActivateMembership(VideoClub, Now);

        Assert.Equal(2, customer.Memberships.Count);
        Assert.True(customer.HasAccessTo(ProductCategory.Book));
        Assert.True(customer.HasAccessTo(ProductCategory.Video));
    }

    [Fact]
    public void Activating_the_same_membership_again_keeps_a_single_membership()
    {
        var customer = NewCustomer();

        customer.ActivateMembership(BookClub, Now);
        customer.ActivateMembership(BookClub, Now.AddDays(1));

        var membership = Assert.Single(customer.Memberships);
        Assert.Equal(Now, membership.ActivatedAt);
    }

    [Fact]
    public void Activated_membership_records_its_product_and_activation_time()
    {
        var customer = NewCustomer();

        customer.ActivateMembership(Premium, Now);

        var membership = Assert.Single(customer.Memberships);
        Assert.Equal(Premium.Id, membership.ProductId);
        Assert.Equal(Now, membership.ActivatedAt);
    }

    [Fact]
    public void Activating_requires_a_membership_product()
    {
        var customer = NewCustomer();

        Assert.Throws<ArgumentNullException>(() => customer.ActivateMembership(null!, Now));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Id_must_be_positive(long id)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Customer(id, "Test Customer"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Name_is_required(string name)
    {
        Assert.Throws<ArgumentException>(() => new Customer(1, name));
    }
}
