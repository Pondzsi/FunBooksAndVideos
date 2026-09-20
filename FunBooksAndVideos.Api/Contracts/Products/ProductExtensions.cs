using FunBooksAndVideos.Domain.Products;

namespace FunBooksAndVideos.Api.Contracts.Products;

public static class ProductExtensions
{
    // The category of a product, or for a membership the categories it grants.
    public static IReadOnlyList<ProductCategory> ToCategories(this Product product)
    {
        return product switch
        {
            PhysicalProduct physical => [physical.Category],
            DigitalProduct digital => [digital.Category],
            MembershipProduct membership => membership.GrantedCategories.Order().ToList(),
            _ => [],
        };
    }
}
