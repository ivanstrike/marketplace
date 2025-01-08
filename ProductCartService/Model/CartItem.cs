namespace ProductCartMicroservice.Model
{
    public class CartItem
    {
        public Guid Id { get; set; }
        public string Name{ get; set; } 
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public Guid CartId { get; set; }
        public Guid ProductId { get; set; }
    }
}
