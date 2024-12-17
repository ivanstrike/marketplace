using CatalogMicroservice.Data;
using CatalogMicroservice.Model;
using Microsoft.EntityFrameworkCore;

namespace CatalogMicroservice.Services
{
    public class CatalogService : ICatalogService
    {
        private readonly DbContextClass _context;
        private readonly IMinioService _minioService;

        public CatalogService(DbContextClass context, IMinioService minioService)
        {
            _context = context;
            _minioService = minioService;
        }

        public async Task<IEnumerable<Product>> GetProductListAsync()
        {
            return await _context.Products.ToListAsync();
        }

        public async Task<Product?> GetProductByIdAsync(Guid id)
        {
            return await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Product> CreateProductAsync(ProductDTO productDto, Guid creatorId)
        {
            
            Guid productId = Guid.NewGuid();

            string imageUrl = await _minioService.UploadFileAsync(productDto.ImageFile);
            if (string.IsNullOrEmpty(imageUrl))
                throw new InvalidOperationException("Image upload failed.");

            
            Product newProduct = new Product
            {
                Id = productId,
                Name = productDto.Name,
                Description = productDto.Description,
                Price = productDto.Price,
                ImageUrl = imageUrl,
                ProductCreatorId = creatorId
            };

            await _context.Products.AddAsync(newProduct);
            await _context.SaveChangesAsync();

            return newProduct;
        }

        public async Task<bool> DeleteProductAsync(Guid id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return false;
            }

       
            await _minioService.DeleteFileAsync(product.ImageUrl);

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<Product?> UpdateProductAsync(Guid id, ProductDTO updatedProduct)
        {
            var product = await _context.Products.FirstOrDefaultAsync(u => u.Id == id);
            if (product == null) return null;
            string newImageUrl = await _minioService.UploadFileAsync(updatedProduct.ImageFile);
            if (!string.IsNullOrEmpty(newImageUrl))
            {
                
                await _minioService.DeleteFileAsync(product.ImageUrl);

                product.ImageUrl = newImageUrl;
            }

         
            product.Name = updatedProduct.Name;
            product.Description = updatedProduct.Description;
            product.Price = updatedProduct.Price;

         
            await _context.SaveChangesAsync();
            return product;
        }
    }
}
