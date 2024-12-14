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
            _channel = _connection.CreateModel();
        }

        public Task PublishMessageAsync(string queueName, object message)
        {
            _channel.QueueDeclare(queueName, durable: true, exclusive: false, autoDelete: false);

            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
            _channel.BasicPublish(exchange: "", routingKey: queueName, basicProperties: null, body: body);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _channel.Close();
            _connection.Close();
        }
    }
}
