using CatalogMicroservice.Data;
using CatalogMicroservice.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace CatalogMicroservice.Repository
{
    public class ProductRepository : IProductRepository
    {
        private readonly DbContextClass _context;
        private readonly IDistributedCache _cache;

        public ProductRepository(DbContextClass context, IDistributedCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public async Task<IEnumerable<Product>> GetAllProductsAsync()
        {
            const string cacheKey = "ProductList";
            var cachedProducts = await _cache.GetStringAsync(cacheKey);

            if (cachedProducts != null)
            {
                return JsonSerializer.Deserialize<IEnumerable<Product>>(cachedProducts)!;
            }

            var products = await _context.Products.ToListAsync();

            if (products.Any())
            {
                var cacheEntryOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
                };

                await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(products), cacheEntryOptions);
            }

            return products;
        }

        public async Task<Product?> GetProductByIdAsync(Guid id)
        {
            var cacheKey = $"Product_{id}";
            var cachedProduct = await _cache.GetStringAsync(cacheKey);

            if (cachedProduct != null)
            {
                return JsonSerializer.Deserialize<Product>(cachedProduct);
            }

            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);

            if (product != null)
            {
                var cacheEntryOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
                };

                await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(product), cacheEntryOptions);
            }

            return product;
        }

        public async Task<Product> CreateProductAsync(Product product)
        {
            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();


            const string cacheKey = "ProductList";
            await _cache.RemoveAsync(cacheKey);

            return product;
        }


        public async Task<bool> DeleteProductAsync(Guid productId)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                return false;
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();


            var cacheKey = $"Product_{productId}";
            await _cache.RemoveAsync(cacheKey);
            await _cache.RemoveAsync("ProductList");

            return true;
        }


        public async Task<Product?> UpdateProductAsync(Guid productId, UpdateProductDTO updatedProduct)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == productId);
            if (product == null) return null;

            product.Name = updatedProduct.Name;
            product.Description = updatedProduct.Description;
            product.Price = updatedProduct.Price;
            product.ImageUrl = updatedProduct.ImageUrl;

            await _context.SaveChangesAsync();


            var cacheKey = $"Product_{product.Id}";
            await _cache.SetStringAsync(
                cacheKey,
                JsonSerializer.Serialize(product),
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30) }
            );


            await _cache.RemoveAsync("ProductList");

            return product;
        }
    }
}

