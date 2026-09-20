using FunBooksAndVideos.Domain.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FunBooksAndVideos.Infrastructure.Persistence.Configurations;

internal sealed class MembershipProductConfiguration : IEntityTypeConfiguration<MembershipProduct>
{
    public void Configure(EntityTypeBuilder<MembershipProduct> builder)
    {
        builder.Property(product => product.GrantedCategories)
            .HasConversion(ProductCategorySet.Converter, ProductCategorySet.Comparer)
            .HasMaxLength(50);
    }
}
