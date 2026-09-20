using FunBooksAndVideos.Domain.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FunBooksAndVideos.Infrastructure.Persistence.Configurations;

internal sealed class PhysicalProductConfiguration : IEntityTypeConfiguration<PhysicalProduct>
{
    public void Configure(EntityTypeBuilder<PhysicalProduct> builder)
    {
        // Same column as DigitalProduct.Category, so it must be configured identically.
        builder.Property(product => product.Category).HasColumnName("Category").HasConversion<string>().HasMaxLength(20);
    }
}
