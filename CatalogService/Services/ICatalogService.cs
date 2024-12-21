using CatalogMicroservice.Model;

namespace CatalogMicroservice.Services
{
    public interface ICatalogService
    {
        Task<IEnumerable<Product>> GetProductListAsync();
        Task<Product?> GetProductByIdAsync(Guid id);
        Task<Product> CreateProductAsync(Guid creatorId, ProductDTO productDto);
        Task<Product> AddToCartAsync(Guid carttId, Guid productId);
        Task<Product?> UpdateProductAsync(Guid id, ProductDTO updatedProduct);
        Task<bool> DeleteProductAsync(Guid id);
    }
}
