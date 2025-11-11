using Dapr;
using Microsoft.AspNetCore.Mvc;
using TSWMS.ProductService.Shared.Interfaces.EventHandlers;
using TSWMS.ProductService.Shared.Models.Events;

namespace TSWMS.ProductService.Api.Controllers;

[Route("api/product-events")]
[ApiController]
public class ProductEventsController : ControllerBase
{
    private readonly IOrderCreatedEventHandler _orderCreatedEventHandler;

    public ProductEventsController(IOrderCreatedEventHandler orderCreatedHandler)
    {
        _orderCreatedEventHandler = orderCreatedHandler;
    }

    [Topic("pubsub", "order.created")]
    [HttpPost("order-created")]
    public async Task<IActionResult> HandleOrderCreatedEvent([FromBody] OrderCreatedEvent @event)
    {
        if (@event == null || @event.OrderItems == null || !@event.OrderItems.Any())
        {
            return BadRequest("Invalid order.created event payload.");
        }

        await _orderCreatedEventHandler.HandleOrderCreatedEventAsync(@event);

        return Ok();
    }

}