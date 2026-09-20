using FunBooksAndVideos.Api.Contracts.Products;
using FunBooksAndVideos.Application.Products;
using Microsoft.AspNetCore.Mvc;

namespace FunBooksAndVideos.Api.Controllers;

[Route("api/v1/products")]
public sealed class ProductsController : ApiControllerBase
{
    private readonly GetProducts _getProducts;
    private readonly GetProduct _getProduct;

    public ProductsController(GetProducts getProducts, GetProduct getProduct)
    {
        _getProducts = getProducts;
        _getProduct = getProduct;
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
