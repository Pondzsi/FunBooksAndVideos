using FunBooksAndVideos.Domain.Customers;
using FunBooksAndVideos.Domain.Products;

namespace FunBooksAndVideos.Api.Contracts.Customers;

public sealed record MembershipResponse(long ProductId, IReadOnlyList<ProductCategory> Categories, DateTimeOffset ActivatedAt)
{
    public static MembershipResponse From(Membership membership)
    {
        return new MembershipResponse(membership.ProductId, membership.GrantedCategories.Order().ToList(), membership.ActivatedAt);
    }
}
