using ProductCartMicroservice.Model;

namespace ProductCartMicroservice.Services
{
    public interface ICartService
    {
        Task<IEnumerable<Cart>> GetCartListAsync();
        
        Task<Cart?> GetCartByIdAsync(Guid id);
    
        Task<Cart> CreateCartAsync(Cart cart);

        Task<Cart?> UpdateCartItemAsync(Guid id, UpdateCartItemDTO updatedCart);

        Task<bool> DeleteCartAsync(Guid id);
 
        Task<Cart?> GetCartByUserIdAsync(Guid userId);

        Task<bool> ClearCartAsync(Guid id);

    }
}

