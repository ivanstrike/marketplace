namespace ProductCartMicroservice.Model
{
    public class CartItem
    {
        public Guid Id { get; set; }
        public int Quantity { get; set; } 
        public decimal Price { get; set; }
    }
}
