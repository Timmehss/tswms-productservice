using TSWMS.ProductService.Shared.Models;

namespace TSWMS.ProductService.Shared.Interfaces;

public interface IProductManager
{
    Task<IEnumerable<Product>> GetProductsAsync();
    Task<IEnumerable<ProductPrice>> GetProductPricesAsync(List<Guid> productIds);
}
