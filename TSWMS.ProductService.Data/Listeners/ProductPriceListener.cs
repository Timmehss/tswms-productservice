#region Usings

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using TSWMS.ProductService.Shared.Helpers;
using TSWMS.ProductService.Shared.Interfaces;
using TSWMS.ProductService.Shared.Models;
using TSWMS.ProductService.Shared.Models.Requests;
using TSWMS.ProductService.Shared.Models.Responses;
using TSWMS.ProductService.Shared.Options;

#endregion

namespace TSWMS.ProductService.Data.Listeners;

public class ProductPriceListener : IProductPriceListener
{
    private readonly IConnectionFactory _connectionFactory;
    private IConnection? _connection;
    private IChannel? _channel;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly string _secretKey;

    public ProductPriceListener(IConnectionFactory connectionFactory, IServiceScopeFactory serviceScopeFactory, IOptions<HmacOptions> hmacOptions)
    {
        _connectionFactory = connectionFactory;
        _serviceScopeFactory = serviceScopeFactory;
        _secretKey = hmacOptions.Value.SecretKey;
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

        // Retrieve the signature from headers
        var receivedSignature = ea.BasicProperties.Headers != null &&
                                ea.BasicProperties.Headers.TryGetValue("X-Signature", out var headerValue)
                                ? Encoding.UTF8.GetString((byte[])headerValue)
                                : null;

        if (string.IsNullOrEmpty(receivedSignature) ||
            !HmacHelper.ValidateHmac(body, receivedSignature, _secretKey))
        {
            Console.WriteLine("Invalid or missing HMAC signature. Rejecting message.");

            // Send an error response back to the ReplyTo queue (optional)
            var errorResponse = new { ErrorMessage = "Invalid HMAC signature" };
            var errorBody = JsonSerializer.SerializeToUtf8Bytes(errorResponse);

            var errorSignature = HmacHelper.GenerateHmac(errorBody, _secretKey);

            var errorProps = new BasicProperties
            {
                CorrelationId = ea.BasicProperties.CorrelationId,
                Headers = new Dictionary<string, object>
                {
                    { "X-Signature", errorSignature }
                }
            };

            // Send the error response to a failure queue or reply-to (adjust accordingly)
            await _channel.BasicPublishAsync(
                exchange: "",
                routingKey: ea.BasicProperties.ReplyTo,
                mandatory: true,
                basicProperties: errorProps,
                body: errorBody
            );

            // Optionally, reject the message without requeueing
            await _channel.BasicRejectAsync(ea.DeliveryTag, false);

            return;
        }

        // Signature valid — process the message

        var request = JsonSerializer.Deserialize<BatchProductPriceRequest>(body);

        if (request == null)
        {
            Console.WriteLine("Invalid message format. Rejecting message.");
            return;
        }

        var listOfProductIds = new List<Guid>(request.ProductIds);

        // Get the product prices
        var productPrices = await GetProductPricesAsync(listOfProductIds);

        // Create the response with the prices
        var response = new BatchProductPriceResponse
        {
            ProductPrices = productPrices
        };

        // Serialize the response to JSON
        var responseBody = JsonSerializer.SerializeToUtf8Bytes(response);

        var responseSignature = HmacHelper.GenerateHmac(responseBody, _secretKey);

        // Send the response back to the ReplyTo queue
        var props = new BasicProperties
        {
            CorrelationId = ea.BasicProperties.CorrelationId,
            Headers = new Dictionary<string, object>
            {
                { "X-Signature", responseSignature }
            }
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
