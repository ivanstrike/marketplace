namespace UserMicroservice.RabbitMQ.Events
{
    public class ProductCreatedEvent
    {
        public Guid CreatorId { get; set; }
        public Guid ProductId { get; set; }
    }
}
