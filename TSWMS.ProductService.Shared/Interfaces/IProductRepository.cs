using TSWMS.ProductService.Shared.Models;

namespace TSWMS.ProductService.Shared.Interfaces;

public interface IProductRepository
{
    Task<IEnumerable<Product>> GetProductsAsync();
    Task<IEnumerable<Product>> GetProductsByIdsAsync(IEnumerable<Guid> productIds);
    Task UpdateProductsAvailableStockAsync(IEnumerable<Product> products);
    Task<Product> GetProductByIdAsync(Guid productId);
    Task<Product> UpdateProductAsync(Product product);

}
