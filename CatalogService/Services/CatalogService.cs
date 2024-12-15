using CatalogMicroservice.Data;
using CatalogMicroservice.Model;
using Microsoft.EntityFrameworkCore;

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

        public async Task<Product> CreateProductAsync(ProductDTO productDto, Guid creatorId)
        {
            
            Guid productId = Guid.NewGuid();

            string imageUrl = await _amazonS3Service.UploadFileAsync(productDto.ImageFile);
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

       
            //await _amazonS3Service.DeleteFileAsync(product.ImageUrl);

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<Product?> UpdateProductAsync(Guid id, ProductDTO updatedProduct)
        {
            var product = await _context.Products.FirstOrDefaultAsync(u => u.Id == id);
            if (product == null) return null;
            string newImageUrl = await _amazonS3Service.UploadFileAsync(updatedProduct.ImageFile);
            if (!string.IsNullOrEmpty(newImageUrl))
            {
                // Удаляем старое изображение
                //await _amazonS3Service.DeleteFileAsync(product.ImageUrl);

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
