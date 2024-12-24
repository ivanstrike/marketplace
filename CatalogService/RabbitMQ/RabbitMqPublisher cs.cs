using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using global::RabbitMQ.Client;
using CatalogMicroservice.Utility;

namespace ProductCartMicroservice.RabbitMQ
{
    public class RabbitMqPublisher : IDisposable
    {
        private readonly RabbitMqSettings _rabbitMqSettings;
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public RabbitMqPublisher(IOptions<RabbitMqSettings> options)
        {
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

            // Объявляем exchange для отправки сообщений
            _channel.ExchangeDeclare(exchange: "cart.exchange", type: ExchangeType.Topic, durable: true);
            _channel.ExchangeDeclare(exchange: "user.exchange", type: ExchangeType.Topic, durable: true);
        }

        public Task PublishMessageAsync(string exchange, string routingKey, object message)
        {
            try
            {
                var properties = _channel.CreateBasicProperties();
                properties.Persistent = true;

                var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

                Console.WriteLine($"Publishing message to exchange: {exchange}, routingKey: {routingKey}, message: {message}");

                _channel.BasicPublish(exchange: exchange, routingKey: routingKey, basicProperties: properties, body: body);

                Console.WriteLine("Message published successfully.");
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error publishing message: {ex.Message}");
                throw;
            }
        }

        public void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
        }
    }

}
