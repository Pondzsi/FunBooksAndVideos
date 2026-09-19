using FunBooksAndVideos.Domain.Products;

namespace FunBooksAndVideos.Domain.Customers;

public sealed class Membership
{
    public Membership(MembershipProduct product, DateTimeOffset activatedAt)
    {
        ArgumentNullException.ThrowIfNull(product);

        ProductId = product.Id;
        // Copied, so later catalog changes don't alter what the customer was given.
        GrantedCategories = product.GrantedCategories.ToHashSet();
        ActivatedAt = activatedAt;
    }

    public long ProductId { get; }

    public IReadOnlySet<ProductCategory> GrantedCategories { get; }

    public DateTimeOffset ActivatedAt { get; }
}
