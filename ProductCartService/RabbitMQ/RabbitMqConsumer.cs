using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using ProductCartMicroservice.Services;
using ProductCartMicroservice.Model;
using ProductCartMicroservice.RabbitMQ.Events;
using ProductCartMicroservice.Utility;
using Microsoft.Extensions.Options;

public class RabbitMqConsumer : IDisposable
{
    
    private readonly RabbitMqSettings _rabbitMqSettings;
    private readonly IServiceProvider _serviceProvider;
    private readonly IConnection _connection;
    private readonly IModel _channel;

    public RabbitMqConsumer(IServiceProvider serviceProvider, IOptions<RabbitMqSettings> options)
    {
        _serviceProvider = serviceProvider;
        _rabbitMqSettings = options.Value;

        var factory = new ConnectionFactory
        {
            HostName = _rabbitMqSettings.Host,
            Port = _rabbitMqSettings.Port,
            UserName = _rabbitMqSettings.Username,
            Password = _rabbitMqSettings.Password
        };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        // Exchange and queue setup
        _channel.ExchangeDeclare(exchange: "user.exchange", type: ExchangeType.Topic);
        var queueName = _channel.QueueDeclare().QueueName;
        _channel.QueueBind(queue: queueName, exchange: "user.exchange", routingKey: "user.created");

        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += async (model, ea) =>
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var cartService = scope.ServiceProvider.GetRequiredService<ICartService>();

                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var userCreatedEvent = JsonSerializer.Deserialize<UserCreatedEvent>(message);

                if (userCreatedEvent != null)
                {
                    var cart = await cartService.CreateCartAsync(new Cart
                    {
                        UserId = userCreatedEvent.UserId,
                        Items = new List<CartItem>()
                    });

                    // Publish CartCreatedEvent back to RabbitMQ
                    var cartCreatedEvent = new CartCreatedEvent
                    {
                        UserId = userCreatedEvent.UserId,
                        CartId = cart.Id
                    };

                    PublishCartCreatedEvent(cartCreatedEvent);
                }
            }
        };

        _channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);
        
    }

    private void PublishCartCreatedEvent(CartCreatedEvent cartCreatedEvent)
    {
        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(cartCreatedEvent));

        _channel.BasicPublish(
            exchange: "cart.exchange",
            routingKey: "cart.created",
            basicProperties: null,
            body: body);

        
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
}
