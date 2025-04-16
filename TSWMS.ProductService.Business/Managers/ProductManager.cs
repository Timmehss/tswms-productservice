using TSWMS.ProductService.Shared.Interfaces;
using TSWMS.ProductService.Shared.Models;
using TSWMS.ProductService.Shared.Models.Requests;

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

    public async Task UpdateProductsAvailableStockAsync(IEnumerable<UpdateProductStock> stockUpdates)
    {
        // Retrieve current product stock levels
        var productIds = stockUpdates.Select(stockUpdate => stockUpdate.ProductId).Distinct().ToList();
        var products = await _productRepository.GetProductsByIdsAsync(productIds);

        foreach (var product in products)
        {
            var stockUpdateProduct = stockUpdates.FirstOrDefault(stockUpdate => stockUpdate.ProductId == product.ProductId);
            if (stockUpdateProduct != null)
            {
                product.AvailableStock -= stockUpdateProduct.QuantityOrdered;
                if (product.AvailableStock < 0)
                {
                    throw new InvalidOperationException($"Not enough stock for product {product.ProductId}.");
                }
            }
        }

        // Update stock levels in the database
        await _productRepository.UpdateProductsAvailableStockAsync(products);
    }
}
