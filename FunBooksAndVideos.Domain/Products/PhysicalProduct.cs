namespace FunBooksAndVideos.Domain.Products;

public sealed class PhysicalProduct : Product
{
    public PhysicalProduct(long id, string name, decimal price, ProductCategory category)
        : base(id, name, price)
    {
        Category = category;
    }

    public ProductCategory Category { get; }
}
