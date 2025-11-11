namespace TSWMS.ProductService.Shared.Models.DTOs;

public class UpdateProductStockDto
{
    public Guid ProductId { get; set; }
    public int QuantityOrdered { get; set; }
}