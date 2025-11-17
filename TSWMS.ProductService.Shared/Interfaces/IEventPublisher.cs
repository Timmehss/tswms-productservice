namespace TSWMS.ProductService.Shared.Interfaces;

public interface IEventPublisher
{
    Task PublishAsync<TEvent>(TEvent @event) where TEvent : class;
}