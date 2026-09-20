using FunBooksAndVideos.Domain.Customers;
using FunBooksAndVideos.Domain.Products;
using Microsoft.EntityFrameworkCore;

namespace FunBooksAndVideos.Infrastructure.Persistence;

// Inserts the demo catalog and customer into an empty database. Products 1 to 3 and customer 4567890 are the PDF's example order.
public sealed class Seeder
{
    private readonly AppDbContext _context;

    public Seeder(AppDbContext context)
    {
        _context = context;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (!await _context.Products.AnyAsync(cancellationToken))
        {
            _context.Products.AddRange(
                new DigitalProduct(1, "Comprehensive First Aid Training", 24.00m, ProductCategory.Video),
                new PhysicalProduct(2, "The Girl on the train", 9.50m, ProductCategory.Book),
                new MembershipProduct(3, "Book Club Membership", 15.00m, [ProductCategory.Book]),
                new MembershipProduct(4, "Video Club Membership", 15.00m, [ProductCategory.Video]),
                new MembershipProduct(5, "Premium Membership", 25.00m, [ProductCategory.Book, ProductCategory.Video]),
                new PhysicalProduct(6, "Clean Code", 29.99m, ProductCategory.Book),
                new DigitalProduct(7, "Async and Await Explained", 14.99m, ProductCategory.Video));
        }

        if (!await _context.Customers.AnyAsync(cancellationToken))
        {
            _context.Customers.Add(new Customer(4567890, "Demo Customer"));
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
