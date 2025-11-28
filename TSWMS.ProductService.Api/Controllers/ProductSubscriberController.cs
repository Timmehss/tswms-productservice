using Microsoft.AspNetCore.Mvc;
using TSWMS.ProductService.Shared.Interfaces.EventHandlers;

namespace TSWMS.ProductService.Api.Controllers;

[Route("api/product-events")]
[ApiController]
public class ProductSubscriberController : ControllerBase
{
    private readonly IOrderCreatedEventHandler _orderCreatedEventHandler;

    public ProductSubscriberController(IOrderCreatedEventHandler orderCreatedHandler)
    {
        _orderCreatedEventHandler = orderCreatedHandler;
    }

    //[Topic("pubsub", "order.created")]
    //[HttpPost("order-created")]
    //public async Task<IActionResult> ReceiveOrderCreatedEvent([FromBody] OrderCreatedEvent @event)
    //{
    //    // Basic payload validation
    //    if (@event == null)
    //    {
    //        return BadRequest("Event payload is null.");
    //    }
    //    if (@event.OrderItems == null || !@event.OrderItems.Any())
    //    {
    //        return BadRequest("Event contains no order items.");
    //    }

    //    // Delegate all business logic to the business layer
    //    await _orderCreatedEventHandler.HandleOrderCreatedEventAsync(@event);

    //    return Ok();
    //}

}