using CatalogMicroservice.Model;
using CatalogMicroservice.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CatalogMicroservice.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CatalogController : ControllerBase
    {
        private readonly ICatalogService _catalogService;

        public CatalogController(ICatalogService catalogService)
        {
            _catalogService = catalogService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllProducts()
        {
            var products = await _catalogService.GetProductListAsync();
            return Ok(products);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetProductById(Guid id)
        {
            var product = await _catalogService.GetProductByIdAsync(id);
            if (product == null)
                return NotFound();

            return Ok(product);
        }


        [HttpPost("create-product")]
        [Authorize]
        public async Task<IActionResult> CreateProduct([FromForm] ProductDTO productDto)
        {
            var cartIdClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "UserId");
            if (cartIdClaim == null)
            {
                return Unauthorized(new { message = "UserId is missing in the token." });
            }

            var creatorId = Guid.Parse(cartIdClaim.Value);

            var product = await _catalogService.CreateProductAsync(creatorId, productDto);
            return Ok(product);
        }

        [HttpPost("add-to-cart")]
        [Authorize]
        public async Task<IActionResult> AddToCart(Guid productId)
        {
            var cartIdClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "CartId");
            if (cartIdClaim == null)
            {
                return Unauthorized(new { message = "CartId is missing in the token." });
            }

            var cartId = Guid.Parse(cartIdClaim.Value);
            var product = await _catalogService.AddToCartAsync(cartId, productId);

            return Ok(product);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateProduct(Guid id, [FromForm] ProductDTO productDto)
        {
            var updatedProduct = await _catalogService.UpdateProductAsync(id, productDto);
            if (updatedProduct == null)
                return NotFound();

            return Ok(updatedProduct);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteProduct(Guid id)
        {
            bool isDeleted = await _catalogService.DeleteProductAsync(id);
            if (!isDeleted)
                return NotFound();

            return NoContent();
        }
    }
}
