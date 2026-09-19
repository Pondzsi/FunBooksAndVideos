using FunBooksAndVideos.Domain.Common;
using FunBooksAndVideos.Domain.Products;

namespace FunBooksAndVideos.Domain.Customers;

public sealed class Customer : AggregateRoot
{
    private readonly List<Membership> _memberships = [];

    public Customer(long id, string name)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Id = id;
        Name = name;
    }

    public long Id { get; }

    public string Name { get; }

    public IReadOnlyList<Membership> Memberships => _memberships.AsReadOnly();

    public void ActivateMembership(MembershipProduct product, DateTimeOffset activatedAt)
    {
        ArgumentNullException.ThrowIfNull(product);

        if (_memberships.Any(membership => membership.ProductId == product.Id))
        {
            return;
        }

        _memberships.Add(new Membership(product, activatedAt));
        Raise(new MembershipActivated(Id, product.Id));
    }

    public bool HasAccessTo(ProductCategory category)
    {
        return _memberships.Any(membership => membership.GrantedCategories.Contains(category));
    }
}
