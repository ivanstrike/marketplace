namespace ProductCartMicroservice.RabbitMQ.Events
{
    public class CartCreatedEvent
    {
        public Guid UserId { get; set; }
        public Guid CartId { get; set; }
    }
}
