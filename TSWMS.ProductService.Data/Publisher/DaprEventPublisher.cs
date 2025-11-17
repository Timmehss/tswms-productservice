using Dapr.Client;
using Microsoft.Extensions.Configuration;
using TSWMS.ProductService.Shared.Interfaces;
using TSWMS.ProductService.Shared.Models.Events;

namespace TSWMS.ProductService.Data.Publishers;

public class DaprEventPublisher : IEventPublisher
{
    private readonly DaprClient _daprClient;

    private readonly string _pubSubName;
    private readonly Dictionary<Type, string> _eventTopics;

    public DaprEventPublisher(DaprClient daprClient, IConfiguration config)
    {
        _daprClient = daprClient;
        _pubSubName = config["Dapr:ComponentNames:PubSub"];

        _eventTopics = new Dictionary<Type, string>
        {
            { typeof(ProductUpdatedEvent), config["Dapr:Topics:Products:ProductUpdated"] }
        };
    }

    public async Task PublishAsync<TEvent>(TEvent @event) where TEvent : class
    {
        var type = typeof(TEvent);
        if (!_eventTopics.TryGetValue(type, out var topicName))
        {
            throw new InvalidOperationException($"No topic configured for event {type.Name}");
        }

        await _daprClient.PublishEventAsync(_pubSubName, topicName, @event);
    }

}