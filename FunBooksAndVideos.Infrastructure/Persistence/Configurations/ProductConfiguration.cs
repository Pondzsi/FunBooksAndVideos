using FunBooksAndVideos.Domain.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FunBooksAndVideos.Infrastructure.Persistence.Configurations;

// All product types share one table, told apart by the "Kind" column.
internal sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(product => product.Id);
        builder.Property(product => product.Id).ValueGeneratedNever();
        builder.Property(product => product.Name).IsRequired().HasMaxLength(200);
        builder.Property(product => product.Price).HasPrecision(18, 2);

        builder.HasDiscriminator<string>("Kind")
            .HasValue<PhysicalProduct>("Physical")
            .HasValue<DigitalProduct>("Digital")
            .HasValue<MembershipProduct>("Membership");
        builder.Property<string>("Kind").HasMaxLength(20);
    }
}
