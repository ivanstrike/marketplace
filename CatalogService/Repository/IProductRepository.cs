using CatalogMicroservice.Model;

namespace CatalogMicroservice.Repository
{
    public interface IProductRepository
    {
        Task<IEnumerable<Product>> GetAllProductsAsync();
        Task<Product?> GetProductByIdAsync(Guid productId);
        Task<IEnumerable<Product>> GetProductsByNameAsync(string name);
        Task<Product> CreateProductAsync(Product product);
        Task<Product?> UpdateProductAsync(Guid productId, UpdateProductDTO product);
        Task<bool> DeleteProductAsync(Guid productId);
    }
}
