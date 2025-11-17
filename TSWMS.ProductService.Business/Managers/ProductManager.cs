using FluentResults;
using TSWMS.ProductService.Shared.Interfaces;
using TSWMS.ProductService.Shared.Models;
using TSWMS.ProductService.Shared.Models.DTOs;
using TSWMS.ProductService.Shared.Models.Events;

namespace TSWMS.ProductService.Business.Managers;

public class ProductManager : IProductManager
{
    private readonly IProductRepository _productRepository;
    private readonly IEventPublisher _eventPublisher;

    public ProductManager(IProductRepository productRepository, IEventPublisher eventPublisher)
    {
        _productRepository = productRepository;
        _eventPublisher = eventPublisher;
    }

    public async Task<IEnumerable<Product>> GetProductsAsync()
    {
        return await _productRepository.GetProductsAsync();
    }

    public async Task<IEnumerable<Product>> GetProductsByIdsAsync(IEnumerable<Guid> productIds)
    {
        return await _productRepository.GetProductsByIdsAsync(productIds);
    }

    public async Task<Result> UpdateProductsAvailableStockAsync(IEnumerable<UpdateProductStockDto> stockUpdates)
    {
        var products = await _productRepository.GetProductsByIdsAsync(stockUpdates.Select(s => s.ProductId));
        var errors = new List<string>();

        var stockUpdateDict = stockUpdates.ToDictionary(s => s.ProductId);

        foreach (var product in products)
        {
            if (stockUpdateDict.TryGetValue(product.ProductId, out var update))
            {
                if (product.AvailableStock < update.QuantityOrdered)
                    errors.Add($"Not enough stock for {product.ProductId}");
                else
                    product.AvailableStock -= update.QuantityOrdered;
            }
        }

        if (errors.Any())
        {
            return Result.Fail(string.Join("; ", errors));
        }

        await _productRepository.UpdateProductsAvailableStockAsync(products);

        return Result.Ok();
    }

    public async Task<Result<Product>> UpdateProductAsync(UpdateProductDto updateProductDto)
    {
        // Get original product for updating
        var originalProduct = await _productRepository.GetProductByIdAsync(updateProductDto.ProductId);
        if (originalProduct == null)
        {
            return Result.Fail("Failed to retrieve original product for updating.");
        }

        // Update fields
        originalProduct.Name = updateProductDto.Name;
        originalProduct.Price = updateProductDto.Price;

        // Save changes
        var updatedProduct = await _productRepository.UpdateProductAsync(originalProduct);
        if (updatedProduct == null)
        {
            return Result.Fail("Failed to update product.");
        }

        // Create ProductUpdatedEvent
        var productUpdatedEvent = new ProductUpdatedEvent
        {
            ProductId = updatedProduct.ProductId,
        };

        // Publish product.updated message
        await _eventPublisher.PublishAsync(productUpdatedEvent);

        return updatedProduct;
    }

}
