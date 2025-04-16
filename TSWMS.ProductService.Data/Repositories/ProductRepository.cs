using Microsoft.EntityFrameworkCore;
using TSWMS.ProductService.Shared.Interfaces;
using TSWMS.ProductService.Shared.Models;

namespace TSWMS.ProductService.Data.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly ProductsDbContext _productDbContext;

    public ProductRepository(ProductsDbContext productDbContext)
    {
        _productDbContext = productDbContext;
    }

    public async Task<IEnumerable<Product>> GetProductsAsync()
    {
        return await _productDbContext.Products.ToListAsync();
    }

    public async Task<IEnumerable<ProductPrice>> GetProductPricesAsync(List<Guid> productIds)
    {
        return await _productDbContext.Products
            .Where(p => productIds.Contains(p.ProductId))
            .Select(p => new ProductPrice
            {
                ProductId = p.ProductId,
                UnitPrice = p.Price
            })
            .ToListAsync();
    }
}
