using System.ComponentModel.DataAnnotations;
using FunBooksAndVideos.Application.Products;
using FunBooksAndVideos.Domain.Products;

namespace FunBooksAndVideos.Api.Contracts.Products;

// A physical or digital product belongs to exactly one category. A membership grants one or more.
public sealed record CreateProductRequest(
    [Required] ProductKind? Kind,
    [Required, StringLength(200, MinimumLength = 1)] string Name,
    [Range(0.01, 1_000_000, ErrorMessage = "Price must be between 0.01 and 1,000,000.")] decimal Price,
    [Required, MinLength(1, ErrorMessage = "Categories must contain at least one category.")] IReadOnlyList<ProductCategory> Categories)
    : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Kind is ProductKind.Physical or ProductKind.Digital && Categories?.Count != 1)
        {
            yield return new ValidationResult($"A {Kind} product belongs to exactly one category.", [nameof(Categories)]);
        }
    }
}
