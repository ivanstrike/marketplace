namespace UserMicroservice.RabbitMQ.Events
{
    public class ProductChangedEvent
    {
        public Guid CreatorId { get; set; }
        public Guid ProductId { get; set; }
    }
}
