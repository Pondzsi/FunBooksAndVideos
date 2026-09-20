using FunBooksAndVideos.Api.Contracts.PurchaseOrders;
using FunBooksAndVideos.Api.Contracts.Shipping;
using FunBooksAndVideos.Application.Orders;
using FunBooksAndVideos.Application.Shipping;
using Microsoft.AspNetCore.Mvc;

namespace FunBooksAndVideos.Api.Controllers;

[Route("api/v1/purchase-orders")]
public sealed class PurchaseOrdersController : ApiControllerBase
{
    private readonly PlacePurchaseOrder _placePurchaseOrder;
    private readonly GetPurchaseOrder _getPurchaseOrder;
    private readonly GetPurchaseOrders _getPurchaseOrders;
    private readonly GetShippingSlip _getShippingSlip;

    public PurchaseOrdersController(
        PlacePurchaseOrder placePurchaseOrder,
        GetPurchaseOrder getPurchaseOrder,
        GetPurchaseOrders getPurchaseOrders,
        GetShippingSlip getShippingSlip)
    {
        _placePurchaseOrder = placePurchaseOrder;
        _getPurchaseOrder = getPurchaseOrder;
        _getPurchaseOrders = getPurchaseOrders;
        _getShippingSlip = getShippingSlip;
    }

    /// <summary>Places a purchase order for a customer. Memberships in it are activated and a shipping slip is generated for physical products.</summary>
    [HttpPost]
    [ProducesResponseType<PlacedPurchaseOrderResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<PlacedPurchaseOrderResponse>> Place(PlacePurchaseOrderRequest request, CancellationToken cancellationToken)
    {
        var result = await _placePurchaseOrder.ExecuteAsync(
            new PlacePurchaseOrderCommand(request.CustomerId, request.ProductIds),
            cancellationToken);

        var response = PlacedPurchaseOrderResponse.From(result);

        return CreatedAtAction(nameof(Get), new { id = response.Id }, response);
    }

    /// <summary>Lists purchase orders, oldest first. Pass customerId to see only one customer's orders.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<PurchaseOrderResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PurchaseOrderResponse>>> GetAll([FromQuery] long? customerId, CancellationToken cancellationToken)
    {
        var orders = await _getPurchaseOrders.ExecuteAsync(customerId, cancellationToken);

        return orders.Select(PurchaseOrderResponse.From).ToList();
    }

    /// <summary>Gets a purchase order with the price each item had when it was placed.</summary>
    [HttpGet("{id:long}")]
    [ProducesResponseType<PurchaseOrderResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PurchaseOrderResponse>> Get(long id, CancellationToken cancellationToken)
    {
        var order = await _getPurchaseOrder.ExecuteAsync(id, cancellationToken);

        if (order is null)
        {
            return NotFoundProblem("Purchase order not found", $"Purchase order {id} was not found.");
        }

        return PurchaseOrderResponse.From(order);
    }

    /// <summary>Gets the shipping slip generated for a purchase order. Orders without physical products have none.</summary>
    [HttpGet("{id:long}/shipping-slip")]
    [ProducesResponseType<ShippingSlipResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ShippingSlipResponse>> GetShippingSlip(long id, CancellationToken cancellationToken)
    {
        var slip = await _getShippingSlip.ExecuteAsync(id, cancellationToken);

        if (slip is null)
        {
            return NotFoundProblem("Shipping slip not found", $"Purchase order {id} has no shipping slip.");
        }

        return ShippingSlipResponse.From(slip);
    }
}
