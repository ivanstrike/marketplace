using CatalogMicroservice.Data;
using CatalogMicroservice.Model;
using CatalogMicroservice.RabbitMQ.Events;
using CatalogMicroservice.Repository;
using ProductCartMicroservice.RabbitMQ;

namespace CatalogMicroservice.Services
{
    public class CatalogService : ICatalogService
    {
        private readonly IMinioService _minioService;
        private readonly IProductRepository _productRepository;
        private readonly RabbitMqPublisher _publisher;

        public CatalogService(IProductRepository productRepository, IMinioService minioService, RabbitMqPublisher publisher)
        {
            _productRepository = productRepository;
            _minioService = minioService;
            _publisher = publisher;
        }

        public async Task<IEnumerable<Product>> GetProductListAsync()
        {
            return await _productRepository.GetAllProductsAsync();
        }

        public async Task<IEnumerable<Product>> GetProductsByNameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Product name cannot be null or empty.", nameof(name));
            }

            var products = await _productRepository.GetProductsByNameAsync(name);

            if (!products.Any())
            {
                throw new KeyNotFoundException($"No products found with name containing '{name}'.");
            }

            return products;
        }
        public async Task<Product?> GetProductByIdAsync(Guid productId)
        {
            var product = await _productRepository.GetProductByIdAsync(productId);
            if (product == null)
                throw new KeyNotFoundException($"Product with ID {productId} not found.");

            return product;
        }

        public async Task<Product> CreateProductAsync(Guid creatorId, ProductDTO productDto)
        {
            if (productDto == null)
                throw new ArgumentException("ProductDTO cannot be null.");

            // Загружаем изображение в Minio
            var imageUrl = await _minioService.UploadFileAsync(productDto.ImageFile);
            if (string.IsNullOrEmpty(imageUrl))
                throw new InvalidOperationException("Image upload failed.");

            // Создаем продукт
            var newProduct = new Product
            {
                Id = Guid.NewGuid(),
                Name = productDto.Name ?? throw new ArgumentException("Product name cannot be null."),
                Description = productDto.Description,
                Price = productDto.Price,
                ImageUrl = imageUrl,
                ProductCreatorId = creatorId
            };

            var createdProduct = await _productRepository.CreateProductAsync(newProduct);

            // Публикуем событие
            var message = new
            {
                CreatorId = creatorId,
                ProductId = createdProduct.Id,
            };
            await _publisher.PublishMessageAsync("user.exchange", "product_created", message);

            return createdProduct;
        }

        public async Task<Product> AddToCartAsync(Guid cartId, Guid productId)
        {
            var product = await GetProductByIdAsync(productId);
            try
            {
                await _publisher.PublishMessageAsync("cart.exchange", "cart.item_added", new ProductAddedToCartEvent
                {
                    CartId = cartId,
                    ProductId = product.Id,
                    Name = product.Name,
                    Price = product.Price
                });
                return product;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<bool> DeleteProductAsync(Guid creatorId, Guid productId)
        {
            var product = await GetProductByIdAsync(productId);

            // Удаляем файл из Minio
            await _minioService.DeleteFileAsync(product.ImageUrl);

            // Публикуем события
            var deleteEvent = new ProductDeletedEvent
            {
                CreatorId = creatorId,
                ProductId = product.Id
            };
            await _publisher.PublishMessageAsync("cart.exchange", "product_deleted", deleteEvent);
            await _publisher.PublishMessageAsync("user.exchange", "product_deleted", deleteEvent);

            // Удаляем продукт
            return await _productRepository.DeleteProductAsync(productId);
        }

        public async Task<Product?> UpdateProductAsync(Guid productId, ProductDTO updatedProduct)
        {
            if (updatedProduct == null)
                throw new ArgumentException("Updated product data cannot be null.");

            var product = await GetProductByIdAsync(productId);

            string newImageUrl = await _minioService.UploadFileAsync(updatedProduct.ImageFile);
            if (!string.IsNullOrEmpty(newImageUrl))
            {
                await _minioService.DeleteFileAsync(product.ImageUrl);
            }

            var productForUpdate = new UpdateProductDTO
            {
                Name = updatedProduct.Name ?? throw new ArgumentException("Product name cannot be null."),
                Description = updatedProduct.Description,
                Price = updatedProduct.Price,
                ImageUrl = newImageUrl,
            };

            return await _productRepository.UpdateProductAsync(productId, productForUpdate);
        }
    }
}
