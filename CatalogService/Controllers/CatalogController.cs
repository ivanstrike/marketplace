using CatalogMicroservice.Model;
using CatalogMicroservice.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Xml.Linq;

namespace CatalogMicroservice.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CatalogController : ControllerBase
    {
        private readonly ICatalogService _catalogService;
        private readonly ILogger<CatalogController> _logger;

        public CatalogController(ICatalogService catalogService, ILogger<CatalogController> logger)
        {
            _catalogService = catalogService;
            _logger = logger;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAllProducts()
        {
            _logger.LogInformation("Fetching all products.");
            var products = await _catalogService.GetProductListAsync();
            return Ok(products);
        }

        [HttpGet("by-name")]
        [Authorize]
        public async Task<IActionResult> GetProductsByName([FromQuery] string name)
        {
            var products = await _catalogService.GetProductsByNameAsync(name);
            _logger.LogInformation("Fetched {Count} products with name containing '{Name}'.", products.Count(), name);
            return Ok(products);
        }

        [HttpGet("{id:guid}")]
        [Authorize]
        public async Task<IActionResult> GetProductById(Guid id)
        {
            _logger.LogInformation("Fetching product with ID {Id}.", id);
            var product = await _catalogService.GetProductByIdAsync(id);
            _logger.LogInformation("Fetched product with ID {Id}.", id);
            return Ok(product);
        }

        [HttpPost("create-product")]
        [Authorize]
        public async Task<IActionResult> CreateProduct([FromForm] ProductDTO productDto)
        {
            var creatorId = GetUserIdFromToken();
            _logger.LogInformation("Creating product by user {UserId}.", creatorId);

            var product = await _catalogService.CreateProductAsync(creatorId, productDto);
            _logger.LogInformation("Product created successfully by user {UserId} with ID {ProductId}.", creatorId, product.Id);

            return CreatedAtAction(nameof(GetProductById), new { id = product.Id }, product);
        }

        [HttpPost("add-to-cart")]
        [Authorize]
        public async Task<IActionResult> AddToCart(Guid productId)
        {
            var cartId = GetCartIdFromToken();
            _logger.LogInformation("Adding product {ProductId} to cart {CartId}.", productId, cartId);

            var product = await _catalogService.AddToCartAsync(cartId, productId);
            _logger.LogInformation("Product {ProductId} added to cart {CartId} successfully.", productId, cartId);

            return Ok(product);
        }

        [HttpPut("{id:guid}")]
        [Authorize]
        public async Task<IActionResult> UpdateProduct(Guid id, [FromForm] ProductDTO productDto)
        {
            _logger.LogInformation("Updating product with ID {Id}.", id);
            var updatedProduct = await _catalogService.UpdateProductAsync(id, productDto);
            _logger.LogInformation("Product with ID {Id} updated successfully.", id);

            return Ok(updatedProduct);
        }

        [HttpDelete("{productId:guid}")]
        [Authorize]
        public async Task<IActionResult> DeleteProduct(Guid productId)
        {
            var creatorId = GetUserIdFromToken();
            _logger.LogInformation("Deleting product with ID {ProductId} by user {CreatorId}.", productId, creatorId);

            bool isDeleted = await _catalogService.DeleteProductAsync(creatorId, productId);
            if (!isDeleted)
            {
                _logger.LogWarning("Product with ID {ProductId} not found for deletion.", productId);
                return NotFound();
            }

            _logger.LogInformation("Product with ID {ProductId} deleted successfully.", productId);
            return NoContent();
        }

        private Guid GetUserIdFromToken()
        {
            var userIdClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "UserId");
            if (userIdClaim == null)
            {
                _logger.LogWarning("UserId is missing in the token.");
                throw new UnauthorizedAccessException("UserId is missing in the token.");
            }

            return Guid.Parse(userIdClaim.Value);
        }

        private Guid GetCartIdFromToken()
        {
            var cartIdClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "CartId");
            if (cartIdClaim == null)
            {
                _logger.LogWarning("CartId is missing in the token.");
                throw new UnauthorizedAccessException("CartId is missing in the token.");
            }

            return Guid.Parse(cartIdClaim.Value);
        }
    }
}
