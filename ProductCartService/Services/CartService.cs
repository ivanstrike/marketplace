using ProductCartMicroservice.Model;
using ProductCartMicroservice.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Cors.Infrastructure;
using ProductCartMicroservice.RabbitMQ.Events;

namespace ProductCartMicroservice.Services
{
    public class CartService : ICartService
    {
        private readonly DbContextClass _context;

        public CartService(DbContextClass context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Cart>> GetCartListAsync()
        {
            return await _context.Carts.Include(c => c.Items).ToListAsync();
        }

        public async Task<Cart?> GetCartByIdAsync(Guid id)
        {
            return await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Cart?> GetCartByUserIdAsync(Guid userId)
        {
            return await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);
        }

        public async Task<Cart> CreateCartAsync(Cart cart)
        {
            cart.Id = Guid.NewGuid();
            cart.CreatedAt = DateTime.UtcNow;

            _context.Carts.Add(cart);
            await _context.SaveChangesAsync();
            return cart;
        }

        public async Task<CartItem> AddToCart(CartItemAddedEvent addedProduct)
        {
            var cart = await GetCartByIdAsync(addedProduct.CartId);
            if (cart == null)
            {
                throw new Exception($"Cart with ID {addedProduct.CartId} not found.");
            }

            
            if (cart.Items.Any(item => item.ProductId == addedProduct.ProductId))
            {
                throw new Exception("This product is already in the cart.");
            }

            var newItem = new CartItem
            {
                Id = Guid.NewGuid(),
                Name = addedProduct.Name,
                Price = addedProduct.Price,
                ProductId = addedProduct.ProductId
            };

            cart.Items.Add(newItem);
            _context.CartItems.Add(newItem);

            await _context.SaveChangesAsync();

            return newItem;
        }

        public async Task<bool> DeleteCartAsync(Guid id)
        {
            var cart = await GetCartByIdAsync(id);
            if (cart == null) return false;
            _context.CartItems.RemoveRange(cart.Items);
            _context.Carts.Remove(cart);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ClearCartAsync(Guid id)
        {
            var cart = await GetCartByIdAsync(id);
            if (cart == null) return false;

            _context.CartItems.RemoveRange(cart.Items);
            cart.Items.Clear();

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
