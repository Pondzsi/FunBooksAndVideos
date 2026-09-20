using FunBooksAndVideos.Domain.Products;

namespace FunBooksAndVideos.Application.Products;

// Categories is the one category of a physical or digital product, or the categories a membership grants.
public sealed record CreateProductCommand(ProductKind Kind, string Name, decimal Price, IReadOnlyCollection<ProductCategory> Categories);
