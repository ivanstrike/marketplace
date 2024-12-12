namespace ProductCartMicroservice.RabbitMQ.Events
{
    public class UserCreatedEvent
    {
        public Guid UserId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
    }
}
