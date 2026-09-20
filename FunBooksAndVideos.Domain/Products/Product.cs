namespace FunBooksAndVideos.Domain.Products;

public abstract class Product
{
    // For EF Core.
    protected Product()
    {
        Name = null!;
    }

    protected Product(long id, string name, decimal price)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(price);

        Id = id;
        Name = name;
        Price = price;
    }

    public long Id { get; }

    public string Name { get; }

    public decimal Price { get; }
}
