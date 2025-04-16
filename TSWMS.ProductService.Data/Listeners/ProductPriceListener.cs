#region Usings

using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text.Json;
using TSWMS.ProductService.Shared.Interfaces;
using TSWMS.ProductService.Shared.Models;
using TSWMS.ProductService.Shared.Models.Requests;
using TSWMS.ProductService.Shared.Models.Responses;

#endregion

namespace TSWMS.ProductService.Data.Listeners;

public class ProductPriceListener : IProductPriceListener
{
    private readonly IConnectionFactory _connectionFactory;
    private IConnection? _connection;
    private IChannel? _channel;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public ProductPriceListener(IConnectionFactory connectionFactory, IServiceScopeFactory serviceScopeFactory)
    {
        _connectionFactory = connectionFactory;
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task InitializeAsync()
    {
        _connection = await _connectionFactory.CreateConnectionAsync();
        _channel = await _connection.CreateChannelAsync();

        // Declare the queue to ensure it exists
        await _channel.QueueDeclareAsync(
            queue: "product.price.request",
            durable: true,
            exclusive: false,
            autoDelete: false
        );

        // Start listening for incoming requests
        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += HandlePriceRequestAsync;

        // Begin consuming messages from the queue
        await _channel.BasicConsumeAsync(
            queue: "product.price.request",
            autoAck: true,
            consumer: consumer
        );
    }

    private async Task HandlePriceRequestAsync(object model, BasicDeliverEventArgs ea)
    {
        var body = ea.Body.ToArray();
        var request = JsonSerializer.Deserialize<BatchProductPriceRequest>(body);

        if (request == null)
        {
            throw new InvalidOperationException("Invalid message received.");
        }

        var listOfProductIds = new List<Guid>();
        listOfProductIds.AddRange(request.ProductIds);

        // Get the product prices
        var productPrices = await GetProductPricesAsync(listOfProductIds);

        // Create the response with the prices
        var response = new BatchProductPriceResponse
        {
            ProductPrices = productPrices
        };

        // Serialize the response to JSON
        var responseBody = JsonSerializer.SerializeToUtf8Bytes(response);

        // Send the response back to the ReplyTo queue
        var props = new BasicProperties
        {
            CorrelationId = ea.BasicProperties.CorrelationId,
        };

        await _channel.BasicPublishAsync(
            exchange: "",
            routingKey: ea.BasicProperties.ReplyTo,
            mandatory: true,
            basicProperties: props,
            body: responseBody
        );
    }

    private async Task<List<ProductPrice>> GetProductPricesAsync(List<Guid> productIds)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var _productRepository = scope.ServiceProvider.GetRequiredService<IProductRepository>();

        // Fetch the actual product prices from the repository
        var products = await _productRepository.GetProductsByIdsAsync(productIds);

        var productPrices = products.Select(product => new ProductPrice
        {
            ProductId = product.ProductId,
            UnitPrice = product.Price
        }).ToList();

        // Convert to List if it's not already
        return productPrices.ToList();
    }
}
