using TSWMS.ProductService.Shared.Interfaces;
using TSWMS.ProductService.Shared.Models;

namespace TSWMS.ProductService.Business.Managers;

public class ProductManager : IProductManager
{
    private readonly IProductRepository _productRepository;
    public ProductManager(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<IEnumerable<Product>> GetProducts()
    {
        return await _productRepository.GetProducts();
    }

    public async Task<IEnumerable<ProductPrice>> GetProductPricesAsync(List<Guid> productIds)
    {
        return await _productRepository.GetProductPricesAsync(productIds);
    }
}
