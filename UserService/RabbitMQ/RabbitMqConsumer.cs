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

        // Exchange and queue setup
        _channel.ExchangeDeclare(exchange: "cart.exchange", type: ExchangeType.Topic);
        var queueName = _channel.QueueDeclare().QueueName;
        _channel.QueueBind(queue: queueName, exchange: "cart.exchange", routingKey: "cart.created");

        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += async (model, ea) =>
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var cartCreatedEvent = JsonSerializer.Deserialize<CartCreatedEvent>(message);

                if (cartCreatedEvent != null)
                {
                    // Обновление пользователя
                    var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
                    await userService.UpdateProductCartIdAsync(cartCreatedEvent.UserId, cartCreatedEvent.CartId);
                }
            }
        };

        _channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
}
