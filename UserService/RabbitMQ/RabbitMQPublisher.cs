using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

namespace UserMicroservice.RabbitMQ
{
    public class RabbitMqPublisher
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public RabbitMqPublisher()
        {
            var factory = new ConnectionFactory { HostName = "localhost" }; // Укажите ваш хост
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
