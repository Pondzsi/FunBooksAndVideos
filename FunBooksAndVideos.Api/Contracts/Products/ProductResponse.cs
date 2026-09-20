using FunBooksAndVideos.Domain.Products;

namespace FunBooksAndVideos.Api.Contracts.Products;

// Categories is the category a product belongs to, or for a membership the categories it grants access to.
public sealed record ProductResponse(long Id, string Name, decimal Price, ProductKind Kind, IReadOnlyList<ProductCategory> Categories)
{
    public static ProductResponse From(Product product)
    {
        IReadOnlyList<ProductCategory> categories = product switch
        {
            PhysicalProduct physical => [physical.Category],
            DigitalProduct digital => [digital.Category],
            MembershipProduct membership => membership.GrantedCategories.Order().ToList(),
            _ => [],
        };

        return new ProductResponse(product.Id, product.Name, product.Price, product.ToKind(), categories);
    }
}
