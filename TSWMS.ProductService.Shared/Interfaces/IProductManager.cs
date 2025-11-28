using FluentResults;
using TSWMS.ProductService.Shared.Models;
using TSWMS.ProductService.Shared.Models.DTOs;

namespace TSWMS.ProductService.Shared.Interfaces;

public interface IProductManager
{
    Task<IEnumerable<Product>> GetProductsAsync();
    Task<IEnumerable<Product>> GetProductsByIdsAsync(IEnumerable<Guid> productIds);
    Task<Result> UpdateProductsAvailableStockAsync(IEnumerable<UpdateProductStockDto> stockUpdates);
    Task<Result<Product>> UpdateProductAsync(UpdateProductDto updateProductDto);
    Task<Result> RestoreStockAsync(IEnumerable<UpdateProductStockDto> stockUpdates);
    Task<Result> DeductStockAsync(IEnumerable<UpdateProductStockDto> stockUpdates);

}