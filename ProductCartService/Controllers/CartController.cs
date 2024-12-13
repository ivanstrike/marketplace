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

        // Получение всех корзин
        [HttpGet("list")]
        public async Task<ActionResult<IEnumerable<Cart>>> GetCartsAsync()
        {
            try
            {
                _logger.LogInformation("Fetching all carts.");
                var carts = await _cartService.GetCartListAsync();
                return Ok(carts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching carts.");
                return StatusCode(500, new { message = "Internal server error." });
            }
        }

        // Получение корзины по ID
        [HttpGet("{id}")]
        public async Task<ActionResult<Cart>> GetCartByIdAsync(Guid id)
        {
            try
            {
                _logger.LogInformation("Fetching cart with ID {Id}", id);
                var cart = await _cartService.GetCartByIdAsync(id);
                if (cart == null)
                {
                    _logger.LogWarning("Cart with ID {Id} not found.", id);
                    return NotFound(new { message = $"Cart with ID {id} not found." });
                }

                return Ok(cart);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching cart with ID {Id}", id);
                return StatusCode(500, new { message = "Internal server error." });
            }
        }

        // Получение корзины по UserId
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<Cart>> GetCartByUserIdAsync(Guid userId)
        {
            try
            {
                _logger.LogInformation("Fetching cart for user with ID {UserId}", userId);
                var cart = await _cartService.GetCartByUserIdAsync(userId);
                if (cart == null)
                {
                    _logger.LogWarning("Cart for user with ID {UserId} not found.", userId);
                    return NotFound(new { message = $"Cart for user with ID {userId} not found." });
                }

                return Ok(cart);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching cart for user with ID {UserId}", userId);
                return StatusCode(500, new { message = "Internal server error." });
            }
        }

        // Создание корзины
        [HttpPost]
        public async Task<ActionResult<Cart>> CreateCartAsync([FromBody] Cart cart)
        {
            try
            {
                _logger.LogInformation("Creating a new cart for user with ID {UserId}", cart.UserId);
                var createdCart = await _cartService.CreateCartAsync(cart);
                return CreatedAtAction(nameof(GetCartByIdAsync), new { id = createdCart.Id }, createdCart);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating a new cart.");
                return StatusCode(500, new { message = "Internal server error." });
            }
        }

        // Обновление элемента корзины
        [HttpPut("{cartId}")]
        public async Task<ActionResult<Cart>> UpdateCartItemAsync(Guid cartId, [FromBody] UpdateCartItemDTO itemUpdate)
        {
            try
            {
                _logger.LogInformation("Updating item in cart with ID {CartId}", cartId);
                var updatedCart = await _cartService.UpdateCartItemAsync(cartId, itemUpdate);
                if (updatedCart == null)
                {
                    _logger.LogWarning("Cart with ID {CartId} not found.", cartId);
                    return NotFound(new { message = $"Cart with ID {cartId} not found." });
                }

                return Ok(updatedCart);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating item in cart with ID {CartId}", cartId);
                return StatusCode(500, new { message = "Internal server error." });
            }
        }

        // Удаление корзины
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteCartAsync(Guid id)
        {
            try
            {
                _logger.LogInformation("Deleting cart with ID {Id}", id);
                var result = await _cartService.DeleteCartAsync(id);
                if (!result)
                {
                    _logger.LogWarning("Cart with ID {Id} not found.", id);
                    return NotFound(new { message = $"Cart with ID {id} not found." });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting cart with ID {Id}", id);
                return StatusCode(500, new { message = "Internal server error." });
            }
        }

        // Очистка корзины
        [HttpPost("{id}/clear")]
        public async Task<ActionResult> ClearCartAsync(Guid id)
        {
            try
            {
                _logger.LogInformation("Clearing cart with ID {Id}", id);
                var result = await _cartService.ClearCartAsync(id);
                if (!result)
                {
                    _logger.LogWarning("Cart with ID {Id} not found.", id);
                    return NotFound(new { message = $"Cart with ID {id} not found." });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while clearing cart with ID {Id}", id);
                return StatusCode(500, new { message = "Internal server error." });
            }
        }
    }
}
