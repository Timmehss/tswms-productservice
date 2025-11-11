using TSWMS.ProductService.Shared.Models.DTOs;

namespace TSWMS.ProductService.Shared.Models.Events;

public class OrderCreatedEvent
{
    public Guid OrderId { get; set; }
    public List<OrderItemEventDto> OrderItems { get; set; } = new();
}