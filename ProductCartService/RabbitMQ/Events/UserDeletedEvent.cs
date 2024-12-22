namespace ProductCartMicroservice.RabbitMQ.Events
{
    public class UserDeletedEvent
    {
        public Guid UserId { get; set; }
        public Guid ProdutCartId { get; set; }

    }
}
