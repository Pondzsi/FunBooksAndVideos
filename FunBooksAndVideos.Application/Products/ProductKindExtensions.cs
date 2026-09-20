using FunBooksAndVideos.Domain.Products;

namespace FunBooksAndVideos.Application.Products;

public static class ProductKindExtensions
{
    public static ProductKind ToKind(this Product product)
    {
        return product switch
        {
            PhysicalProduct => ProductKind.Physical,
            DigitalProduct => ProductKind.Digital,
            MembershipProduct => ProductKind.Membership,
            _ => throw new ArgumentOutOfRangeException(nameof(product), product.GetType().Name, "Unknown product type."),
        };
    }
}
