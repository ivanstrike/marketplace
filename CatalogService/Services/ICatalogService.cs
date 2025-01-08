using CatalogMicroservice.Model;

namespace CatalogMicroservice.Services
{
    public interface ICatalogService
    {
        Task<IEnumerable<Product>> GetProductListAsync();
        Task<IEnumerable<Product>> GetProductsByNameAsync(string name);
        Task<Product?> GetProductByIdAsync(Guid id);
        Task<Product> CreateProductAsync(Guid creatorId, ProductDTO productDto);
        Task<Product> AddToCartAsync(Guid cartId, Guid productId);
        Task<Product?> UpdateProductAsync(Guid id, ProductDTO updatedProduct);
        Task<bool> DeleteProductAsync(Guid creatorId, Guid productId);
    }
}
