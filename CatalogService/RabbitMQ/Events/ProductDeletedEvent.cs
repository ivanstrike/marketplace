namespace CatalogMicroservice.RabbitMQ.Events
{
    public class ProductDeletedEvent
    {
        public Guid CreatorId { get; set; }
        public Guid ProductId { get; set; }
    }
}
