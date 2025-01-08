namespace ProductCartMicroservice.RabbitMQ.Events
{
    public class UserDeletedEvent
    {
        public Guid CartId { get; set; }
        public string JwtToken { get; set; }

    }
}
