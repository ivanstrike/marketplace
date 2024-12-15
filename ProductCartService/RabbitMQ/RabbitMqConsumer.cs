using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using ProductCartMicroservice.Services;
using ProductCartMicroservice.Model;
using ProductCartMicroservice.RabbitMQ.Events;
using ProductCartMicroservice.Utility;
using Microsoft.Extensions.Options;

using ProductCartMicroservice.RabbitMQ;

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

        // Dead Letter Exchange
        _channel.ExchangeDeclare(exchange: "dead_letter_exchange", type: ExchangeType.Direct);
        _channel.QueueDeclare(queue: "dead_letter_queue", durable: true, exclusive: false, autoDelete: false);
        _channel.QueueBind(queue: "dead_letter_queue", exchange: "dead_letter_exchange", routingKey: "user.created.dlx");

        // Queue setup with DLX
        var args = new Dictionary<string, object>
        {
            { "x-dead-letter-exchange", "dead_letter_exchange" },
            { "x-dead-letter-routing-key", "user.created.dlx" }
        };

        _channel.QueueDeclare(queue: "user_created_queue", durable: true, exclusive: false, autoDelete: false, arguments: args);
        _channel.QueueBind(queue: "user_created_queue", exchange: "user.exchange", routingKey: "user.created");

        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += async (model, ea) =>
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var cartService = scope.ServiceProvider.GetRequiredService<ICartService>();
                    var rabbitMqPublisher = scope.ServiceProvider.GetRequiredService<RabbitMqPublisher>();

                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    var userCreatedEvent = JsonSerializer.Deserialize<UserCreatedEvent>(message);

                    if (userCreatedEvent != null)
                    {
                        // Создаем корзину
                        var cart = await cartService.CreateCartAsync(new Cart
                        {
                            UserId = userCreatedEvent.UserId,
                            Items = new List<CartItem>()
                        });

                        // Отправляем событие о созданной корзине
                        var cartCreatedEvent = new CartCreatedEvent
                        {
                            UserId = userCreatedEvent.UserId,
                            CartId = cart.Id
                        };

                        rabbitMqPublisher.PublishMessage("cart.created", cartCreatedEvent);
                    }
                }

                // Подтверждение успешной обработки сообщения
                _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing message: {ex.Message}");
                _channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true);
            }
        };

        _channel.BasicConsume(queue: "user_created_queue", autoAck: false, consumer: consumer);
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
}
