using TSWMS.ProductService.Shared.Interfaces;
using TSWMS.ProductService.Shared.Interfaces.EventHandlers;
using TSWMS.ProductService.Shared.Models.DTOs;
using TSWMS.ProductService.Shared.Models.Events;

namespace TSWMS.ProductService.Data.EventHandlers.Orders;

public class OrderCreatedEventHandler : IOrderCreatedEventHandler
{
    private readonly IProductManager _productManager;

    public OrderCreatedEventHandler(IProductManager productManager)
    {
        _productManager = productManager;
    }

    public async Task HandleOrderCreatedEventAsync(OrderCreatedEvent @event)
    {
        var stockUpdates = @event.OrderItems
            .Select(orderItem => new UpdateProductStockDto
            {
                ProductId = orderItem.ProductId,
                QuantityOrdered = orderItem.Quantity
            }).ToList();

        await _productManager.UpdateProductsAvailableStockAsync(stockUpdates);
    }

}