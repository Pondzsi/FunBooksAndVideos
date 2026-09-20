using FunBooksAndVideos.Domain.Customers;
using FunBooksAndVideos.Domain.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FunBooksAndVideos.Infrastructure.Persistence.Configurations;

internal sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");

        builder.HasKey(customer => customer.Id);
        builder.Property(customer => customer.Id).ValueGeneratedNever();
        builder.Property(customer => customer.Name).IsRequired().HasMaxLength(200);
        builder.Ignore(customer => customer.DomainEvents);

        builder.OwnsMany(customer => customer.Memberships, membership =>
        {
            membership.ToTable("Memberships");
            membership.WithOwner().HasForeignKey("CustomerId");

            membership.Property(m => m.ActivatedAt);
            membership.Property(m => m.GrantedCategories)
                .HasConversion(ProductCategorySet.Converter, ProductCategorySet.Comparer)
                .HasMaxLength(50);

            membership.HasOne<Product>().WithMany().HasForeignKey(m => m.ProductId).OnDelete(DeleteBehavior.Restrict);

            // A customer holds a given membership at most once.
            membership.HasIndex("CustomerId", nameof(Membership.ProductId)).IsUnique();
        });

        builder.Navigation(customer => customer.Memberships).HasField("_memberships");
    }
}
