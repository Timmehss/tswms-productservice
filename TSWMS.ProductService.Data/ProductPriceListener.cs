#region Usings

using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text.Json;
using TSWMS.ProductService.Shared.Interfaces;
using TSWMS.ProductService.Shared.Models;
using TSWMS.ProductService.Shared.Models.Requests;

#endregion

namespace TSWMS.ProductService.Data;

public class ProductPriceListener : IProductPriceListener, IAsyncDisposable
{
    private readonly IConnectionFactory _connectionFactory;
    private readonly IProductRepository _productRepository;
    private IConnection? _connection;
    private IChannel? _channel;

    public ProductPriceListener(IConnectionFactory connectionFactory, IProductRepository productRepository)
    {
        _connectionFactory = connectionFactory;
        _productRepository = productRepository;
    }

    // Initialize the RabbitMQ connection and consumer
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

    // Handle incoming product price request
    private async Task HandlePriceRequestAsync(object model, BasicDeliverEventArgs ea)
    {
        var body = ea.Body.ToArray();
        var request = JsonSerializer.Deserialize<BatchProductPriceRequest>(body);

        if (request == null)
        {
            throw new InvalidOperationException("Invalid message received.");
        }

        // Assuming you have a method GetProductPrices to fetch the prices for products
        var productPrices = await GetProductPricesAsync(request.ProductIds);

        // Create the response with the prices
        var response = new BatchProductPriceResponse
        {
            ProductPrices = productPrices
        };

        // Serialize the response to JSON
        var responseBody = JsonSerializer.SerializeToUtf8Bytes(response);

        // Send the response back to the ReplyTo queue (specified in the incoming message)
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

    // Fetch product prices for a list of product IDs
    private async Task<List<ProductPrice>> GetProductPricesAsync(List<Guid> productIds)
    {
        // Fetch the actual product prices from the repository
        var productPrices = await _productRepository.GetProductPricesAsync(productIds);

        // Convert to List if it's not already
        return productPrices.ToList();
    }

    // Implement IAsyncDisposable to clean up resources when done
    public async ValueTask DisposeAsync()
    {
        if (_channel != null)
            await _channel.CloseAsync();

        if (_connection != null)
            await _connection.CloseAsync();

        _channel?.Dispose();
        _connection?.Dispose();
    }
}
