using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductCartMicroservice.Model;
using ProductCartMicroservice.Services;

namespace ProductCartMicroservice.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;
        private readonly ILogger<CartController> _logger;

        public CartController(ICartService cartService, ILogger<CartController> logger)
        {
            _cartService = cartService;
            _logger = logger;
        }

        
        [HttpGet("list")]
        public async Task<ActionResult<IEnumerable<Cart>>> GetCartsAsync()
        {
            _logger.LogInformation("Fetching all carts.");
            var carts = await _cartService.GetCartListAsync();
            return Ok(carts);
        }

        
        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<Cart>> GetCartByIdAsync(Guid id)
        {
            _logger.LogInformation("Fetching cart with ID {Id}", id);
            var cart = await _cartService.GetCartByIdAsync(id);
            return Ok(cart);
        }

        
        [Authorize]
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<Cart>> GetCartByUserIdAsync(Guid userId)
        {
            _logger.LogInformation("Fetching cart for user with ID {UserId}", userId);
            var cart = await _cartService.GetCartByUserIdAsync(userId);
            return Ok(cart);
        }

        
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<Cart>> CreateCartAsync([FromBody] Cart cart)
        {
            _logger.LogInformation("Creating a new cart for user with ID {UserId}", cart.UserId);
            var createdCart = await _cartService.CreateCartAsync(cart);
            return CreatedAtAction(nameof(GetCartByIdAsync), new { id = createdCart.Id }, createdCart);
        }

       
        [Authorize]
        [HttpDelete("items/{cartItemId}/decrease")]
        public async Task<IActionResult> DecreaseCartItemQuantityAsync(Guid cartItemId)
        {
            _logger.LogInformation("Decreasing quantity of CartItem with ID {CartItemId}", cartItemId);
            await _cartService.DecreaseQuantityAsync(cartItemId);
            return NoContent();
        }

     
        [Authorize]
        [HttpDelete("items/{cartItemId}")]
        public async Task<IActionResult> DeleteCartItemAsync(Guid cartItemId)
        {
            _logger.LogInformation("Deleting CartItem with ID {CartItemId}", cartItemId);
            await _cartService.DeleteFromCartAsync(cartItemId);
            return NoContent();
        }

   
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCartAsync(Guid id)
        {
            _logger.LogInformation("Deleting Cart with ID {CartId}", id);
            await _cartService.DeleteCartAsync(id);
            return NoContent();
        }

  
        [Authorize]
        [HttpPost("{id}/clear")]
        public async Task<IActionResult> ClearCartAsync(Guid id)
        {
            _logger.LogInformation("Clearing cart with ID {Id}", id);
            await _cartService.ClearCartAsync(id);
            return NoContent();
        }
    }
}
