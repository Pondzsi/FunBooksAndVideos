using FunBooksAndVideos.Domain.Customers;
using FunBooksAndVideos.Domain.Orders;
using FunBooksAndVideos.Domain.Shipping;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FunBooksAndVideos.Infrastructure.Persistence.Configurations;

internal sealed class ShippingSlipConfiguration : IEntityTypeConfiguration<ShippingSlip>
{
    public void Configure(EntityTypeBuilder<ShippingSlip> builder)
    {
        builder.ToTable("ShippingSlips");

        // One slip per order, so the order's ID is the key.
        builder.HasKey(slip => slip.PurchaseOrderId);
        builder.Property(slip => slip.PurchaseOrderId).ValueGeneratedNever();
        builder.Property(slip => slip.GeneratedAt);
        builder.Ignore(slip => slip.DomainEvents);

        builder.HasOne<PurchaseOrder>().WithOne().HasForeignKey<ShippingSlip>(slip => slip.PurchaseOrderId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Customer>().WithMany().HasForeignKey(slip => slip.CustomerId).OnDelete(DeleteBehavior.Restrict);

        builder.OwnsMany(slip => slip.Items, item =>
        {
            item.ToTable("ShippingSlipItems");
            item.WithOwner().HasForeignKey("PurchaseOrderId");

            item.Property(i => i.ProductName).IsRequired().HasMaxLength(200);
        });

        builder.Navigation(slip => slip.Items).HasField("_items");
    }
}
