using TSWMS.ProductService.Shared.Models.Events;

namespace TSWMS.ProductService.Shared.Interfaces.EventHandlers;

public interface IOrderCreatedEventHandler
{
    Task HandleOrderCreatedEventAsync(OrderCreatedEvent @event);
}