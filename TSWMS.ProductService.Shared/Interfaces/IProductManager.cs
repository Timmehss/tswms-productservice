using TSWMS.ProductService.Shared.Models;
using TSWMS.ProductService.Shared.Models.Requests;

namespace TSWMS.ProductService.Shared.Interfaces;

public interface IProductManager
{
    Task<IEnumerable<Product>> GetProductsAsync();
    Task<IEnumerable<Product>> GetProductsByIdsAsync(IEnumerable<Guid> productIds);
    Task UpdateProductsAvailableStockAsync(IEnumerable<UpdateProductStock> stockUpdates);
}
