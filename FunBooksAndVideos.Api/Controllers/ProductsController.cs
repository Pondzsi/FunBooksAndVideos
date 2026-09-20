using FunBooksAndVideos.Api.Contracts.Products;
using FunBooksAndVideos.Application.Products;
using Microsoft.AspNetCore.Mvc;

namespace FunBooksAndVideos.Api.Controllers;

[Route("api/v1/products")]
public sealed class ProductsController : ApiControllerBase
{
    private readonly CreateProduct _createProduct;
    private readonly GetProducts _getProducts;
    private readonly GetProduct _getProduct;

    public ProductsController(CreateProduct createProduct, GetProducts getProducts, GetProduct getProduct)
    {
        _createProduct = createProduct;
        _getProducts = getProducts;
        _getProduct = getProduct;
    }

    /// <summary>Adds a product to the catalog: a physical or digital product in one category, or a membership that grants one or more.</summary>
    [HttpPost]
    [ProducesResponseType<ProductResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProductResponse>> Create(CreateProductRequest request, CancellationToken cancellationToken)
    {
        var product = await _createProduct.ExecuteAsync(
            new CreateProductCommand(request.Kind!.Value, request.Name, request.Price, request.Categories),
            cancellationToken);

        var response = ProductResponse.From(product);

        return CreatedAtAction(nameof(Get), new { id = response.Id }, response);
    }

    /// <summary>Lists the catalog: books, videos and memberships.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<ProductResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProductResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var products = await _getProducts.ExecuteAsync(cancellationToken);

        return products.Select(ProductResponse.From).ToList();
    }

    /// <summary>Gets one product from the catalog.</summary>
    [HttpGet("{id:long}")]
    [ProducesResponseType<ProductResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductResponse>> Get(long id, CancellationToken cancellationToken)
    {
        var product = await _getProduct.ExecuteAsync(id, cancellationToken);

        if (product is null)
        {
            return NotFoundProblem("Product not found", $"Product {id} was not found.");
        }

        return ProductResponse.From(product);
    }
}
