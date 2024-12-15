namespace ProductCartMicroservice.Model
{
    public class CartUpdatedEvent
    {
        public Guid CartId { get; set; }
        public Guid UserId { get; set; }
        public List<CartItem> Items { get; set; } = new();
        public DateTime UpdatedAt { get; set; }
    }
}
