namespace ProductCartMicroservice.Model
{
    public class UpdateCartItemDTO
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
