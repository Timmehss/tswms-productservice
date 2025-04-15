using TSWMS.ProductService.Shared.Models;

namespace TSWMS.ProductService.Shared.Interfaces;

public interface IProductRepository
{
    Task<IEnumerable<Product>> GetProducts();
    Task<IEnumerable<ProductPrice>> GetProductPricesAsync(List<Guid> productIds);
}
