namespace FunBooksAndVideos.Domain.Products;

public sealed class MembershipProduct : Product
{
    public MembershipProduct(long id, string name, decimal price, IEnumerable<ProductCategory> grantedCategories)
        : base(id, name, price)
    {
        ArgumentNullException.ThrowIfNull(grantedCategories);

        GrantedCategories = grantedCategories.ToHashSet();

        if (GrantedCategories.Count == 0)
        {
            throw new ArgumentException("A membership must grant access to at least one category.", nameof(grantedCategories));
        }
    }

    public IReadOnlySet<ProductCategory> GrantedCategories { get; }
}
