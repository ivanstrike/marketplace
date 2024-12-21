namespace ProductCartMicroservice.Model
{
    public class AddCartItemDTO
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
        public Guid ProductId { get; set; }
    }
}
