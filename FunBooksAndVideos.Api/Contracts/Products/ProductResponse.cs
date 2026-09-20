using FunBooksAndVideos.Application.Products;
using FunBooksAndVideos.Domain.Products;

namespace FunBooksAndVideos.Api.Contracts.Products;

// Categories is the category a product belongs to, or for a membership the categories it grants access to.
public sealed record ProductResponse(long Id, string Name, decimal Price, ProductKind Kind, IReadOnlyList<ProductCategory> Categories)
{
    public static ProductResponse From(Product product)
    {
        return new ProductResponse(product.Id, product.Name, product.Price, product.ToKind(), product.ToCategories());
    }
}
