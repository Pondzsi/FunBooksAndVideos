using FunBooksAndVideos.Api.Contracts.Customers;
using FunBooksAndVideos.Application.Customers;
using Microsoft.AspNetCore.Mvc;

namespace FunBooksAndVideos.Api.Controllers;

[Route("api/v1/customers")]
public sealed class CustomersController : ApiControllerBase
{
    private readonly GetCustomer _getCustomer;

    public CustomersController(GetCustomer getCustomer)
    {
        _getCustomer = getCustomer;
    }

    /// <summary>Gets a customer with their memberships and the categories those memberships give access to.</summary>
    [HttpGet("{id:long}")]
    [ProducesResponseType<CustomerResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CustomerResponse>> Get(long id, CancellationToken cancellationToken)
    {
        var customer = await _getCustomer.ExecuteAsync(id, cancellationToken);

        if (customer is null)
        {
            return NotFoundProblem("Customer not found", $"Customer {id} was not found.");
        }

        return CustomerResponse.From(customer);
    }
}
