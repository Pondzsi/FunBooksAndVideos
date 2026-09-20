using FunBooksAndVideos.Domain.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FunBooksAndVideos.Infrastructure.Persistence.Configurations;

internal sealed class DigitalProductConfiguration : IEntityTypeConfiguration<DigitalProduct>
{
    public void Configure(EntityTypeBuilder<DigitalProduct> builder)
    {
        // Same column as PhysicalProduct.Category, so it must be configured identically.
        builder.Property(product => product.Category).HasColumnName("Category").HasConversion<string>().HasMaxLength(20);
    }
}
