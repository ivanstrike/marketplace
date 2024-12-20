﻿using CatalogMicroservice.Data;
using CatalogMicroservice.Model;
using CatalogMicroservice.Repository;
using Microsoft.EntityFrameworkCore;

namespace CatalogMicroservice.Services
{
    public class CatalogService : ICatalogService
    {
        private readonly IMinioService _minioService;
        private readonly IProductRepository _productRepository;
        private readonly IRabbitMqService _rabbitMqService;

        public CatalogService(IProductRepository productRepository, IMinioService minioService, IRabbitMqService rabbitMqService)
        {
            _productRepository = productRepository;
            _minioService = minioService;
            _rabbitMqService = rabbitMqService;
        }

        public async Task<IEnumerable<Product>> GetProductListAsync()
        {
            return await _productRepository.GetAllProductsAsync();
        }

        public async Task<Product?> GetProductByIdAsync(Guid productId)
        {
            return await _productRepository.GetProductByIdAsync(productId);
        }

        public async Task<Product> CreateProductAsync(ProductDTO productDto, Guid creatorId)
        {
            Guid productId = Guid.NewGuid();

            // Загружаем изображение в Minio
            string imageUrl = await _minioService.UploadFileAsync(productDto.ImageFile);
            if (string.IsNullOrEmpty(imageUrl))
                throw new InvalidOperationException("Image upload failed.");

            // Создаем продукт
            Product newProduct = new Product
            {
                Id = productId,
                Name = productDto.Name,
                Description = productDto.Description,
                Price = productDto.Price,
                ImageUrl = imageUrl,
                ProductCreatorId = creatorId
            };

            var createdProduct = await _productRepository.CreateProductAsync(newProduct);

            var message = new
            {
                UserId = creatorId,
                ProductId = createdProduct.Id,
                Event = "ProductCreated"
            };
            _rabbitMqService.PublishMessage("UserServiceQueue", message);

            return createdProduct;
        }

        public async Task<bool> DeleteProductAsync(Guid productId)
        {
            var product = await _productRepository.GetProductByIdAsync(productId);
            if (product == null)
            {
                return false;
            }

            await _minioService.DeleteFileAsync(product.ImageUrl);

            return await _productRepository.DeleteProductAsync(productId);
        }

        public async Task<Product?> UpdateProductAsync(Guid productId, ProductDTO updatedProduct)
        {
            var product = await _productRepository.GetProductByIdAsync(productId);
            if (product == null) return null;

            string newImageUrl = await _minioService.UploadFileAsync(updatedProduct.ImageFile);
            if (!string.IsNullOrEmpty(newImageUrl))
            {
                await _minioService.DeleteFileAsync(product.ImageUrl);
            }

            var productForUpdate = new UpdateProductDTO
            {
                Name = updatedProduct.Name,
                Description = updatedProduct.Description,
                Price = updatedProduct.Price,
                ImageUrl = newImageUrl,
            };

            return await _productRepository.UpdateProductAsync(productId, productForUpdate);
        }
    }
}
