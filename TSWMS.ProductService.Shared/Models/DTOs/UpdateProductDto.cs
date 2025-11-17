namespace TSWMS.ProductService.Shared.Models.DTOs;

public class UpdateProductDto
{
    public Guid ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
}