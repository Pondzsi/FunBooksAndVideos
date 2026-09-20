using FunBooksAndVideos.Api.Contracts.Customers;
using FunBooksAndVideos.Application.Customers;
using Microsoft.AspNetCore.Mvc;

namespace FunBooksAndVideos.Api.Controllers;

[Route("api/v1/customers")]
public sealed class CustomersController : ApiControllerBase
{
    private readonly CreateCustomer _createCustomer;
    private readonly GetCustomer _getCustomer;
    private readonly GetCustomers _getCustomers;

    public CustomersController(CreateCustomer createCustomer, GetCustomer getCustomer, GetCustomers getCustomers)
    {
        _createCustomer = createCustomer;
        _getCustomer = getCustomer;
        _getCustomers = getCustomers;
    }

    /// <summary>Creates a customer. They start with no memberships.</summary>
    [HttpPost]
    [ProducesResponseType<CustomerResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CustomerResponse>> Create(CreateCustomerRequest request, CancellationToken cancellationToken)
    {
        var customer = await _createCustomer.ExecuteAsync(request.Name, cancellationToken);
        var response = CustomerResponse.From(customer);

        return CreatedAtAction(nameof(Get), new { id = response.Id }, response);
    }

    /// <summary>Lists the customers with their memberships.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<CustomerResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CustomerResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var customers = await _getCustomers.ExecuteAsync(cancellationToken);

        return customers.Select(CustomerResponse.From).ToList();
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
