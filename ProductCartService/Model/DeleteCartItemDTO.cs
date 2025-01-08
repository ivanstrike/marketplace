namespace ProductCartMicroservice.Model
{
    public class DeleteCartItemDTO
    {
        public Guid Id { get; set; }
        public Guid CartId { get; set; }
    }
}
