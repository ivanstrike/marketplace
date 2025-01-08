using ProductCartMicroservice.Model;
using ProductCartMicroservice.Data;
using Microsoft.EntityFrameworkCore;
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
            var cart = await _context.Carts.Include(c => c.Items).FirstOrDefaultAsync(c => c.Id == id);
            if (cart == null) throw new KeyNotFoundException($"Cart with ID {id} not found.");
            return cart;
        }

        public async Task<Cart?> GetCartByUserIdAsync(Guid userId)
        {
            var cart = await _context.Carts.Include(c => c.Items).FirstOrDefaultAsync(c => c.UserId == userId);
            if (cart == null) throw new KeyNotFoundException($"Cart for User with ID {userId} not found.");
            return cart;
        }

        public async Task<Cart> CreateCartAsync(Cart cart)
        {
            cart.Id = Guid.NewGuid();
            cart.CreatedAt = DateTime.UtcNow;
            cart.UpdatedAt = DateTime.UtcNow;

            _context.Carts.Add(cart);
            await _context.SaveChangesAsync();
            return cart;
        }

        public async Task<CartItem> AddToCartAsync(CartItemAddedEvent addedProduct)
        {
            var cart = await GetCartByIdAsync(addedProduct.CartId);

            var existedItem = await _context.CartItems
                .FirstOrDefaultAsync(i => i.CartId == addedProduct.CartId && i.ProductId == addedProduct.ProductId);

            if (existedItem == null)
            {
                var newItem = new CartItem
                {
                    Id = Guid.NewGuid(),
                    Name = addedProduct.Name,
                    Price = addedProduct.Price,
                    CartId = cart.Id,
                    Quantity = 1,
                    ProductId = addedProduct.ProductId
                };
                _context.CartItems.Add(newItem);
                cart.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return newItem;
            }

            existedItem.Quantity += 1;
            cart.UpdatedAt = DateTime.UtcNow;

            _context.CartItems.Update(existedItem);
            await _context.SaveChangesAsync();
            return existedItem;
        }

        public async Task<bool> DeleteFromCartAsync(Guid cartItemId)
        {
            var cartItem = await _context.CartItems.FirstOrDefaultAsync(i => i.Id == cartItemId);
            if (cartItem == null) throw new KeyNotFoundException($"CartItem with ID {cartItemId} not found.");

            _context.CartItems.Remove(cartItem);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DecreaseQuantityAsync(Guid cartItemId)
        {
            var cartItem = await _context.CartItems.FirstOrDefaultAsync(i => i.Id == cartItemId);
            if (cartItem == null) throw new KeyNotFoundException($"CartItem with ID {cartItemId} not found.");

            if (cartItem.Quantity > 1)
            {
                cartItem.Quantity -= 1;
            }
            else
            {
                _context.CartItems.Remove(cartItem);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task DeleteCartItemsByProductIdAsync(Guid productId)
        {
            var cartItems = await _context.CartItems.Where(ci => ci.ProductId == productId).ToListAsync();

            if (cartItems.Any())
            {
                _context.CartItems.RemoveRange(cartItems);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> DeleteCartAsync(Guid id)
        {
            var cart = await GetCartByIdAsync(id);

            _context.CartItems.RemoveRange(cart.Items);
            _context.Carts.Remove(cart);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ClearCartAsync(Guid id)
        {
            var cart = await GetCartByIdAsync(id);

            _context.CartItems.RemoveRange(cart.Items);
            cart.Items.Clear();
            cart.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
