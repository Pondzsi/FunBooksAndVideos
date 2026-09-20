using FunBooksAndVideos.Domain.Customers;
using FunBooksAndVideos.Domain.Orders;
using FunBooksAndVideos.Domain.Products;
using FunBooksAndVideos.Domain.Shipping;
using Microsoft.EntityFrameworkCore;

namespace FunBooksAndVideos.Infrastructure.Persistence;

public sealed class AppDbContext : DbContext
{
    public const string PurchaseOrderIdSequence = "PurchaseOrderIds";

    // The seed data uses customer 4567890 and products 1 to 7, so new products start above them.
    public const string CustomerIdSequence = "CustomerIds";

    public const string ProductIdSequence = "ProductIds";

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();

    public DbSet<Customer> Customers => Set<Customer>();

    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();

    public DbSet<ShippingSlip> ShippingSlips => Set<ShippingSlip>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasSequence<long>(PurchaseOrderIdSequence);
        modelBuilder.HasSequence<long>(CustomerIdSequence);
        modelBuilder.HasSequence<long>(ProductIdSequence).StartsAt(100);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
