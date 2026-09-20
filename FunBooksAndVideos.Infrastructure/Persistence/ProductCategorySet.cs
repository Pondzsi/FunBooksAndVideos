using FunBooksAndVideos.Domain.Products;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace FunBooksAndVideos.Infrastructure.Persistence;

// Stores a set of categories by name ("Book,Video"), so a row reads the same in SQL as in code.
internal static class ProductCategorySet
{
    public static readonly ValueConverter<IReadOnlySet<ProductCategory>, string> Converter = new(
        categories => string.Join(',', categories.OrderBy(category => category)),
        text => (IReadOnlySet<ProductCategory>)text
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(name => Enum.Parse<ProductCategory>(name))
            .ToHashSet());

    public static readonly ValueComparer<IReadOnlySet<ProductCategory>> Comparer = new(
        (left, right) => left!.SetEquals(right!),
        categories => categories.Aggregate(0, (hash, category) => hash ^ category.GetHashCode()),
        categories => categories.ToHashSet());
}
