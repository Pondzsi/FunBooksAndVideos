using FunBooksAndVideos.Application.Common;
using FunBooksAndVideos.Domain.Products;

namespace FunBooksAndVideos.Application.Products;

public sealed class CreateProduct
{
    private readonly IProductRepository _products;
    private readonly IUnitOfWork _unitOfWork;

    public CreateProduct(IProductRepository products, IUnitOfWork unitOfWork)
    {
        _products = products;
        _unitOfWork = unitOfWork;
    }

    public async Task<Product> ExecuteAsync(CreateProductCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var product = Build(await _products.NextIdAsync(cancellationToken), command);

        await _products.AddAsync(product, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return product;
    }

    private static Product Build(long id, CreateProductCommand command)
    {
        return command.Kind switch
        {
            ProductKind.Physical => new PhysicalProduct(id, command.Name, command.Price, OnlyCategory(command)),
            ProductKind.Digital => new DigitalProduct(id, command.Name, command.Price, OnlyCategory(command)),
            ProductKind.Membership => new MembershipProduct(id, command.Name, command.Price, command.Categories),
            _ => throw new ArgumentOutOfRangeException(nameof(command), command.Kind, "Unknown product kind."),
        };
    }

    private static ProductCategory OnlyCategory(CreateProductCommand command)
    {
        if (command.Categories.Count != 1)
        {
            throw new ArgumentException($"A {command.Kind} product belongs to exactly one category.", nameof(command));
        }

        return command.Categories.Single();
    }
}
