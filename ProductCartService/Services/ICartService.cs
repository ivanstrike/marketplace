using ProductCartMicroservice.Model;
using ProductCartMicroservice.RabbitMQ.Events;

namespace ProductCartMicroservice.Services
{
    public interface ICartService
    {
        Task<IEnumerable<Cart>> GetCartListAsync();
        
        Task<Cart?> GetCartByIdAsync(Guid id);
    
        Task<Cart> CreateCartAsync(Cart cart);

        Task<CartItem> AddToCart(CartItemAddedEvent addedProduct);

        Task<bool> DeleteCartAsync(Guid id);
 
        Task<Cart?> GetCartByUserIdAsync(Guid userId);

        Task<bool> ClearCartAsync(Guid id);

    }
}

