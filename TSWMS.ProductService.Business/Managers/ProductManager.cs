using FluentResults;
using TSWMS.ProductService.Shared.Interfaces;
using TSWMS.ProductService.Shared.Models;
using TSWMS.ProductService.Shared.Models.DTOs;

namespace TSWMS.ProductService.Business.Managers;

public class ProductManager : IProductManager
{
    private readonly IProductRepository _productRepository;

    public ProductManager(IProductRepository productRepository)
    {
        _productRepository = productRepository;
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
}
