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
        _channel.QueueBind(queue: "dead_letter_queue", exchange: "dead_letter_exchange", routingKey: "user.deleted.dlx");

        // Queue setup with DLX for user.deleted
        var args = new Dictionary<string, object>
        {
            { "x-dead-letter-exchange", "dead_letter_exchange" },
            { "x-dead-letter-routing-key", "user.deleted.dlx" }
        };

        _channel.QueueDeclare(queue: "user_deleted_queue", durable: true, exclusive: false, autoDelete: false, arguments: args);
        _channel.QueueBind(queue: "user_deleted_queue", exchange: "user.exchange", routingKey: "user.deleted");

        // Queue setup with DLX for user.created
        args = new Dictionary<string, object>
        {
            { "x-dead-letter-exchange", "dead_letter_exchange" },
            { "x-dead-letter-routing-key", "user.created.dlx" }
        };

        _channel.QueueDeclare(queue: "user_created_queue", durable: true, exclusive: false, autoDelete: false, arguments: args);
        _channel.QueueBind(queue: "user_created_queue", exchange: "user.exchange", routingKey: "user.created");

       
        args = new Dictionary<string, object>
        {
            { "x-dead-letter-exchange", "dead_letter_exchange" },
            { "x-dead-letter-routing-key", "cart.item_added.dlx" }
        };

        _channel.QueueDeclare(queue: "cart_item_added_queue", durable: true, exclusive: false, autoDelete: false, arguments: args);
        _channel.QueueBind(queue: "cart_item_added_queue", exchange: "cart.exchange", routingKey: "cart.item_added");

        // Consumer for cart.item_added
        var cartItemAddedConsumer = new EventingBasicConsumer(_channel);
        cartItemAddedConsumer.Received += async (model, ea) =>
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var cartService = scope.ServiceProvider.GetRequiredService<ICartService>();

                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    var cartItemAddedEvent = JsonSerializer.Deserialize<CartItemAddedEvent>(message);

                    if (cartItemAddedEvent != null)
                    {

                        await cartService.AddToCart(cartItemAddedEvent);

                    }
                }

                // Подтверждение успешной обработки сообщения
                _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing cart.item_added message: {ex.Message}");
                _channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true);
            }
        };

        _channel.BasicConsume(queue: "cart_item_added_queue", autoAck: false, consumer: cartItemAddedConsumer);


        // Consumer for user.created
        var userCreatedConsumer = new EventingBasicConsumer(_channel);
        userCreatedConsumer.Received += async (model, ea) =>
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

                        await rabbitMqPublisher.PublishMessageAsync("cart.created", cartCreatedEvent);
                    }
                }

                // Подтверждение успешной обработки сообщения
                _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing user.created message: {ex.Message}");
                _channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true);
            }
        };

        _channel.BasicConsume(queue: "user_created_queue", autoAck: false, consumer: userCreatedConsumer);

        // Consumer for user.deleted
        var userDeletedConsumer = new EventingBasicConsumer(_channel);
        userDeletedConsumer.Received += async (model, ea) =>
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var cartService = scope.ServiceProvider.GetRequiredService<ICartService>();

                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    var userDeletedEvent = JsonSerializer.Deserialize<UserDeletedEvent>(message);

                    if (userDeletedEvent != null)
                    {
                        
                        await cartService.DeleteCartAsync(userDeletedEvent.CartId);
                    }
                }

                
                _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing user.deleted message: {ex.Message}");
                _channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true);
            }
        };

        _channel.BasicConsume(queue: "user_deleted_queue", autoAck: false, consumer: userDeletedConsumer);
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
}
