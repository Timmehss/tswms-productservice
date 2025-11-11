using TSWMS.ProductService.Shared.Models.DTOs;

namespace TSWMS.ProductService.Shared.Models.Requests;

public class UpdateProductStockRequest
{
    public List<UpdateProductStockDto> UpdateProductStocks { get; set; } = new List<UpdateProductStockDto>();
}