namespace FunBooksAndVideos.Domain.Products;

public sealed class DigitalProduct : Product
{
    public DigitalProduct(long id, string name, decimal price, ProductCategory category)
        : base(id, name, price)
    {
        Category = category;
    }

    public ProductCategory Category { get; }
}
