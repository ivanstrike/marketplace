using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using UserMicroservice.Utility;
using RabbitMQ.Client;
using UserMicroservice.Utility;

namespace UserMicroservice.RabbitMQ
{
    public class RabbitMqPublisher : IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly RabbitMqSettings _rabbitMqSettings;
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
            if (!_connection.IsOpen)
            {
                throw new InvalidOperationException("RabbitMQ connection is not open.");
            }
            _channel = _connection.CreateModel();
            _channel.ExchangeDeclare(exchange: "user.exchange", type: ExchangeType.Topic, durable: true);
        }

        public Task PublishMessageAsync(string routingKey, object message)
        {
            
            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;

           
            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

        
            _channel.BasicPublish(exchange: "user.exchange", routingKey: routingKey, basicProperties: properties, body: body);

            return Task.CompletedTask;
        }


        public void Dispose()
        {
            _channel.Close();
            _connection.Close();
        }
    }
}
