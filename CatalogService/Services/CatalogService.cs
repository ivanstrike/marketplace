using CatalogMicroservice.Data;
using CatalogMicroservice.Model;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CatalogMicroservice.Services
{
    public class CatalogService : ICatalogService
    {
        private readonly DbContextClass _context;
        private readonly AmazonS3Service _amazonS3Service;

        public CatalogService(DbContextClass context, AmazonS3Service amazonS3Service)
        {
            _context = context;
            _amazonS3Service = amazonS3Service;
        }

        public async Task<IEnumerable<Product>> GetProductListAsync()
        {
            return await _context.Products.ToListAsync();
        }

        public async Task<Product?> GetProductByIdAsync(Guid id)
        {
            return await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Product> AddProductAsync(ProductDTO product)
        {
            Guid guid = Guid.NewGuid();
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }
            string imgUrl = await _amazonS3Service.UploadFileAsync(product.ImageFile);
            Product newProduct = new Product
            {
                Id = guid,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                ProductCreatorId = userId,
                ImageUrl = imgUrl
            };
            await _context.Products.AddAsync(newProduct);
            return newProduct;
        }

        public async Task<Product?> UpdateProductAsync(Product product)
        {
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
            return product;
        }

        public async Task<bool> DeleteProductAsync(Guid id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return false;
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
