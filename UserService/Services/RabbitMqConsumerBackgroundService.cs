using Microsoft.Extensions.Hosting;
namespace UserMicroservice.Services
{
    public class RabbitMqConsumerBackgroundService : BackgroundService
    {
        private readonly RabbitMqConsumer _rabbitMqConsumer;

        public RabbitMqConsumerBackgroundService(RabbitMqConsumer rabbitMqConsumer)
        {
            _rabbitMqConsumer = rabbitMqConsumer;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            
            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            _rabbitMqConsumer.Dispose();
            base.Dispose();
        }
    }
}
