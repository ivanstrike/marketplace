using CatalogMicroservice.Model;

namespace CatalogMicroservice.RabbitMQ.Events
{
    public class ProductAddedToCartEvent
    {
        public Guid CartId { get; set; }
        public Guid ProductId { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
    }
}
