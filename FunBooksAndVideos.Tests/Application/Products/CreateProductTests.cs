using FunBooksAndVideos.Application.Common;
using FunBooksAndVideos.Application.Products;
using FunBooksAndVideos.Domain.Products;

namespace FunBooksAndVideos.Tests.Application.Products;

public class CreateProductTests
{
    [Fact]
    public async Task Creates_a_physical_product_in_its_one_category()
    {
        var fixture = new Fixture();

        var product = await fixture.UseCase.ExecuteAsync(new(ProductKind.Physical, "Refactoring", 39.99m, [ProductCategory.Book]), CancellationToken.None);

        var physical = Assert.IsType<PhysicalProduct>(product);
        Assert.Equal((100L, "Refactoring", 39.99m, ProductCategory.Book), (physical.Id, physical.Name, physical.Price, physical.Category));
    }

    [Fact]
    public async Task Creates_a_digital_product_in_its_one_category()
    {
        var fixture = new Fixture();

        var product = await fixture.UseCase.ExecuteAsync(new(ProductKind.Digital, "Async in depth", 14.99m, [ProductCategory.Video]), CancellationToken.None);

        var digital = Assert.IsType<DigitalProduct>(product);
        Assert.Equal(ProductCategory.Video, digital.Category);
    }

    [Fact]
    public async Task Creates_a_membership_granting_all_the_categories_given()
    {
        var fixture = new Fixture();

        var product = await fixture.UseCase.ExecuteAsync(
            new(ProductKind.Membership, "Premium Plus", 30m, [ProductCategory.Book, ProductCategory.Video]),
            CancellationToken.None);

        var membership = Assert.IsType<MembershipProduct>(product);
        Assert.Equal(new[] { ProductCategory.Book, ProductCategory.Video }, membership.GrantedCategories.Order());
    }

    [Fact]
    public async Task Adds_the_product_and_commits_once()
    {
        var fixture = new Fixture();

        var product = await fixture.UseCase.ExecuteAsync(new(ProductKind.Digital, "Async in depth", 14.99m, [ProductCategory.Video]), CancellationToken.None);

        Assert.Same(product, Assert.Single(fixture.Products.Added));
        Assert.Equal(1, fixture.UnitOfWork.Commits);
    }

    [Theory]
    [InlineData(ProductKind.Physical, 0)]
    [InlineData(ProductKind.Physical, 2)]
    [InlineData(ProductKind.Digital, 0)]
    [InlineData(ProductKind.Digital, 2)]
    public async Task A_physical_or_digital_product_needs_exactly_one_category_and_nothing_is_saved_otherwise(ProductKind kind, int categoryCount)
    {
        var fixture = new Fixture();
        var categories = new[] { ProductCategory.Book, ProductCategory.Video }.Take(categoryCount).ToList();

        await Assert.ThrowsAsync<ArgumentException>(
            () => fixture.UseCase.ExecuteAsync(new(kind, "Invalid", 10m, categories), CancellationToken.None));

        Assert.Empty(fixture.Products.Added);
        Assert.Equal(0, fixture.UnitOfWork.Commits);
    }

    private sealed class Fixture
    {
        public Fixture()
        {
            UseCase = new CreateProduct(Products, UnitOfWork);
        }

        public FakeProducts Products { get; } = new();

        public FakeUnitOfWork UnitOfWork { get; } = new();

        public CreateProduct UseCase { get; }
    }

    private sealed class FakeProducts : IProductRepository
    {
        public List<Product> Added { get; } = [];

        public Task<long> NextIdAsync(CancellationToken cancellationToken) => Task.FromResult(100L);

        public Task AddAsync(Product product, CancellationToken cancellationToken)
        {
            Added.Add(product);

            return Task.CompletedTask;
        }

        public Task<Product?> GetByIdAsync(long id, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken) => throw new NotSupportedException();
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public int Commits { get; private set; }

        public Task CommitAsync(CancellationToken cancellationToken)
        {
            Commits++;

            return Task.CompletedTask;
        }
    }
}
