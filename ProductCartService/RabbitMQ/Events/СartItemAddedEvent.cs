namespace ProductCartMicroservice.RabbitMQ.Events
{
    public class CartItemAddedEvent
    {
        public Guid CartId { get; set; }
        public Guid ProductId { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
    }
}
