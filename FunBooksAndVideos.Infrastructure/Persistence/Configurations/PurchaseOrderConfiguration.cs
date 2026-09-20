using FunBooksAndVideos.Domain.Customers;
using FunBooksAndVideos.Domain.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FunBooksAndVideos.Infrastructure.Persistence.Configurations;

internal sealed class PurchaseOrderConfiguration : IEntityTypeConfiguration<PurchaseOrder>
{
    public void Configure(EntityTypeBuilder<PurchaseOrder> builder)
    {
        builder.ToTable("PurchaseOrders");

        // IDs come from the PurchaseOrderIds sequence before the order is built, not from an identity column.
        builder.HasKey(order => order.Id);
        builder.Property(order => order.Id).ValueGeneratedNever();

        builder.HasOne<Customer>().WithMany().HasForeignKey(order => order.CustomerId).OnDelete(DeleteBehavior.Restrict);

        builder.OwnsMany(order => order.Items, item =>
        {
            item.ToTable("PurchaseOrderItems");
            item.WithOwner().HasForeignKey("PurchaseOrderId");

            item.Property(i => i.Price).HasPrecision(18, 2);
            item.HasOne(i => i.Product).WithMany().HasForeignKey("ProductId").IsRequired().OnDelete(DeleteBehavior.Restrict);
        });

        builder.Navigation(order => order.Items).HasField("_items");
    }
}
