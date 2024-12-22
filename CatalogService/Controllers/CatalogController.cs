using CatalogMicroservice.Model;
using CatalogMicroservice.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

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
        public async Task<IActionResult> GetAllProducts()
        {
            _logger.LogInformation("Fetching all products.");
            var products = await _catalogService.GetProductListAsync();
            return Ok(products);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetProductById(Guid id)
        {
            _logger.LogInformation("Fetching product with ID {Id}.", id);
            var product = await _catalogService.GetProductByIdAsync(id);
            if (product == null)
            {
                _logger.LogWarning("Product with ID {Id} not found.", id);
                return NotFound();
            }

            _logger.LogInformation("Fetched product with ID {Id}.", id);
            return Ok(product);
        }

        [HttpPost("create-product")]
        [Authorize]
        public async Task<IActionResult> CreateProduct([FromForm] ProductDTO productDto)
        {
            var userIdClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "UserId");
            if (userIdClaim == null)
            {
                _logger.LogWarning("UserId is missing in the token.");
                return Unauthorized(new { message = "UserId is missing in the token." });
            }

            var creatorId = Guid.Parse(userIdClaim.Value);
            _logger.LogInformation("Creating product by user {UserId}.", creatorId);

            var product = await _catalogService.CreateProductAsync(creatorId, productDto);
            _logger.LogInformation("Product created successfully by user {UserId} with ID {ProductId}.", creatorId, product.Id);

            return Ok(product);
        }

        [HttpPost("add-to-cart")]
        [Authorize]
        public async Task<IActionResult> AddToCart(Guid productId)
        {
            var cartIdClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "CartId");
            if (cartIdClaim == null)
            {
                _logger.LogWarning("CartId is missing in the token.");
                return Unauthorized(new { message = "CartId is missing in the token." });
            }

            var cartId = Guid.Parse(cartIdClaim.Value);
            _logger.LogInformation("Adding product {ProductId} to cart {CartId}.", productId, cartId);

            var product = await _catalogService.AddToCartAsync(cartId, productId);
            _logger.LogInformation("Product {ProductId} added to cart {CartId} successfully.", productId, cartId);

            return Ok(product);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateProduct(Guid id, [FromForm] ProductDTO productDto)
        {
            _logger.LogInformation("Updating product with ID {Id}.", id);
            var updatedProduct = await _catalogService.UpdateProductAsync(id, productDto);
            if (updatedProduct == null)
            {
                _logger.LogWarning("Product with ID {Id} not found for update.", id);
                return NotFound();
            }

            _logger.LogInformation("Product with ID {Id} updated successfully.", id);
            return Ok(updatedProduct);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteProduct(Guid id)
        {
            _logger.LogInformation("Deleting product with ID {Id}.", id);
            bool isDeleted = await _catalogService.DeleteProductAsync(id);
            if (!isDeleted)
            {
                _logger.LogWarning("Product with ID {Id} not found for deletion.", id);
                return NotFound();
            }

            _logger.LogInformation("Product with ID {Id} deleted successfully.", id);
            return NoContent();
        }
    }
}