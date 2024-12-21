namespace ProductCartMicroservice.Model
{
    public class CartItem
    {
        public Guid Id { get; set; }
        public string Name{ get; set; } 
        public decimal Price { get; set; }
        public Guid ProductId { get; set; }
    }
}
