namespace TSWMS.ProductService.Api.Dto;

public class ProductDto
{
    public Guid ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int AvailableStock { get; set; }
}