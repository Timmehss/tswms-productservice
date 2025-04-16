using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text.Json;
using TSWMS.ProductService.Shared.Interfaces;
using TSWMS.ProductService.Shared.Models.Requests;

namespace TSWMS.ProductService.Data.Listeners;

public class UpdateProductStockListener : IUpdateProductStockListener
{
    private readonly IConnectionFactory _connectionFactory;
    private IConnection? _connection;
    private IChannel? _channel;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public UpdateProductStockListener(IConnectionFactory connectionFactory, IServiceScopeFactory serviceScopeFactory)
    {
        _connectionFactory = connectionFactory;
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task InitializeAsync()
    {
        _connection = await _connectionFactory.CreateConnectionAsync();
        _channel = await _connection.CreateChannelAsync();

        // Declare the queue for stock updates
        await _channel.QueueDeclareAsync(
            queue: "product.stock.update",
            durable: true,
            exclusive: false,
            autoDelete: false
        );

        // Start listening for stock update messages
        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += HandleStockUpdateRequestAsync;

        await _channel.BasicConsumeAsync(
            queue: "product.stock.update",
            autoAck: true,
            consumer: consumer
        );
    }

    private async Task HandleStockUpdateRequestAsync(object model, BasicDeliverEventArgs ea)
    {
        var body = ea.Body.ToArray();
        var request = JsonSerializer.Deserialize<UpdateProductStockRequest>(body);

        if (request == null || request.UpdateProductStocks == null)
        {
            throw new InvalidOperationException("Invalid stock update message received.");
        }

        using var scope = _serviceScopeFactory.CreateScope();
        var _productManager = scope.ServiceProvider.GetRequiredService<IProductManager>();

        // Update the products available stock
        await _productManager.UpdateProductsAvailableStockAsync(request.UpdateProductStocks);
    }
}

