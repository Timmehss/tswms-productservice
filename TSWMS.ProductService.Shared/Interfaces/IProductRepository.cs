using TSWMS.ProductService.Shared.Models;

namespace TSWMS.ProductService.Shared.Interfaces;

public interface IProductRepository
{
    Task<IEnumerable<Product>> GetProductsAsync();
    Task<IEnumerable<ProductPrice>> GetProductPricesAsync(List<Guid> productIds);
}
