#region Usings

using TSWMS.ProductService.Shared.Interfaces;

#endregion

namespace TSWMS.ProductService.Data.Listeners;

public class UpdateProductStockListener : IUpdateProductStockListener
{
    //private readonly IConnectionFactory _connectionFactory;
    //private IConnection? _connection;
    //private IChannel? _channel;
    //private readonly IServiceScopeFactory _serviceScopeFactory;
    //private readonly string _secretKey;

    //public UpdateProductStockListener(IConnectionFactory connectionFactory, IServiceScopeFactory serviceScopeFactory, IOptions<HmacOptions> hmacOptions)
    //{
    //    _connectionFactory = connectionFactory;
    //    _serviceScopeFactory = serviceScopeFactory;
    //    _secretKey = hmacOptions.Value.SecretKey;
    //}

    //public async Task InitializeAsync()
    //{
    //    _connection = await _connectionFactory.CreateConnectionAsync();
    //    _channel = await _connection.CreateChannelAsync();

    //    // Declare the queue for stock updates
    //    await _channel.QueueDeclareAsync(
    //        queue: "product.stock.update",
    //        durable: true,
    //        exclusive: false,
    //        autoDelete: false
    //    );

    //    // Start listening for stock update messages
    //    var consumer = new AsyncEventingBasicConsumer(_channel);
    //    consumer.ReceivedAsync += HandleStockUpdateRequestAsync;

    //    await _channel.BasicConsumeAsync(
    //        queue: "product.stock.update",
    //        autoAck: true,
    //        consumer: consumer
    //    );
    //}

    //private async Task HandleStockUpdateRequestAsync(object model, BasicDeliverEventArgs ea)
    //{
    //    var body = ea.Body.ToArray();

    //    // Validate HMAC signature
    //    var receivedSignature = ea.BasicProperties.Headers != null &&
    //                            ea.BasicProperties.Headers.TryGetValue("X-Signature", out var headerValue)
    //                            ? Encoding.UTF8.GetString((byte[])headerValue)
    //                            : null;

    //    if (string.IsNullOrEmpty(receivedSignature) ||
    //        !HmacHelper.ValidateHmac(body, receivedSignature, _secretKey))
    //    {
    //        Console.WriteLine($"[{DateTime.UtcNow}] Invalid or missing HMAC signature on stock update. CorrelationId: {ea.BasicProperties?.CorrelationId}");

    //        return;
    //    }

    //    var request = JsonSerializer.Deserialize<UpdateProductStockRequest>(body);

    //    if (request == null || request.UpdateProductStocks == null)
    //    {
    //        throw new InvalidOperationException("Invalid stock update message received.");
    //    }

    //    using var scope = _serviceScopeFactory.CreateScope();
    //    var _productManager = scope.ServiceProvider.GetRequiredService<IProductManager>();

    //    // Update the products available stock
    //    await _productManager.UpdateProductsAvailableStockAsync(request.UpdateProductStocks);
    //}
}
