using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using UserMicroservice.RabbitMQ.Events;
using UserMicroservice.Services;
using UserMicroservice.Utility;


public class RabbitMqConsumer : IDisposable
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly RabbitMqSettings _rabbitMqSettings;
    public RabbitMqConsumer(IServiceProvider serviceProvider, IOptions<RabbitMqSettings> options)
    {
        _rabbitMqSettings = options.Value;
        _serviceProvider = serviceProvider;

        var factory = new ConnectionFactory
        {
            HostName = _rabbitMqSettings.Host,
            Port = _rabbitMqSettings.Port,
            UserName = _rabbitMqSettings.Username,
            Password = _rabbitMqSettings.Password
        };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        //_channel.ExchangeDeclare(exchange: "cart.exchange", type: ExchangeType.Topic, durable: true);

        // Dead Letter Exchange
        _channel.ExchangeDeclare(exchange: "dead_letter_exchange", type: ExchangeType.Direct);
        _channel.QueueDeclare(queue: "dead_letter_queue", durable: true, exclusive: false, autoDelete: false);
        _channel.QueueBind(queue: "dead_letter_queue", exchange: "dead_letter_exchange", routingKey: "cart.created.dlx");

        // Queue setup with DLX
        var args = new Dictionary<string, object>
        {
        { "x-dead-letter-exchange", "dead_letter_exchange" },
        { "x-dead-letter-routing-key", "cart.created.dlx" }
        };

        _channel.QueueDeclare(queue: "cart_created_queue", durable: true, exclusive: false, autoDelete: false, arguments: args);
        _channel.QueueBind(queue: "cart_created_queue", exchange: "user.exchange", routingKey: "cart.created");

        args = new Dictionary<string, object>
        {
            { "x-dead-letter-exchange", "dead_letter_exchange" },
            { "x-dead-letter-routing-key", "product_created.dlx" }
        };

        _channel.QueueDeclare(queue: "product_created_queue", durable: true, exclusive: false, autoDelete: false, arguments: args);
        _channel.QueueBind(queue: "product_created_queue", exchange: "user.exchange", routingKey: "product_created");

        // Consumer for product_added
        var productCreatedConsumer = new EventingBasicConsumer(_channel);
        productCreatedConsumer.Received += async (model, ea) =>
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    var productCreatedEvent = JsonSerializer.Deserialize<ProductCreatedEvent>(message);

                    if (productCreatedEvent != null)
                    {

                        await userService.CreateProduct(productCreatedEvent.CreatorId, productCreatedEvent.ProductId);

                    }
                }

                // Подтверждение успешной обработки сообщения
                _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing product.created message: {ex.Message}");
                _channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true);
            }
        };

        _channel.BasicConsume(queue: "product_created_queue", autoAck: false, consumer: productCreatedConsumer);

        var cartCreatedConsumer = new EventingBasicConsumer(_channel);
        cartCreatedConsumer.Received += async (model, ea) =>
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                    {
                        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
                        var body = ea.Body.ToArray();
                        var message = Encoding.UTF8.GetString(body);
                        var cartCreatedEvent = JsonSerializer.Deserialize<CartCreatedEvent>(message);

                        if (cartCreatedEvent != null)
                        {
                            await userService.UpdateProductCartIdAsync(cartCreatedEvent.UserId, cartCreatedEvent.CartId);
                        }
                    }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing message: {ex.Message}");
                _channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true);
            }

        };

        _channel.BasicConsume(queue: "cart_created_queue", autoAck: false, consumer: cartCreatedConsumer);
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
}
