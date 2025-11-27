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
        if (stockUpdates == null || !stockUpdates.Any())
        {
            Console.WriteLine("[ProductManager] No stock updates provided.");
            return Result.Fail("No stock updates provided.");
        }

        var products = await _productRepository.GetProductsByIdsAsync(stockUpdates.Select(s => s.ProductId));
        if (products == null || !products.Any())
        {
            Console.WriteLine("[ProductManager] No matching products found for the given IDs.");
            return Result.Fail("No matching products found.");
        }

        var errors = new List<string>();
        var stockUpdateDict = stockUpdates.ToDictionary(s => s.ProductId);

        Console.WriteLine("[ProductManager] Applying stock updates...");
        foreach (var product in products)
        {
            if (stockUpdateDict.TryGetValue(product.ProductId, out var update))
            {
                Console.WriteLine($"[ProductManager] ProductId={product.ProductId} CurrentStock={product.AvailableStock} QuantityChange={update.QuantityChange}");

                product.AvailableStock += update.QuantityChange;

                if (product.AvailableStock < 0)
                {
                    errors.Add($"Not enough stock for ProductId={product.ProductId} (CurrentStock={product.AvailableStock - update.QuantityChange}, RequestedChange={update.QuantityChange})");
                    // Revert the change so stock doesn't go negative
                    product.AvailableStock -= update.QuantityChange;
                    Console.WriteLine($"[ProductManager] Stock update for ProductId={product.ProductId} would go below zero. Change reverted.");
                }
                else
                {
                    Console.WriteLine($"[ProductManager] ProductId={product.ProductId} new AvailableStock={product.AvailableStock}");
                }
            }
            else
            {
                Console.WriteLine($"[ProductManager] No stock update found for ProductId={product.ProductId}");
            }
        }

        if (errors.Any())
        {
            Console.WriteLine($"[ProductManager] Stock update failed: {string.Join("; ", errors)}");
            return Result.Fail(string.Join("; ", errors));
        }

        Console.WriteLine("[ProductManager] Saving updated stock to repository...");
        await _productRepository.UpdateProductsAvailableStockAsync(products);
        Console.WriteLine("[ProductManager] Stock successfully updated.");

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
