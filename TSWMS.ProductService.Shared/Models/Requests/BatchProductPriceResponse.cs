namespace TSWMS.ProductService.Shared.Models.Requests;

public class BatchProductPriceResponse
{
    public List<ProductPrice> ProductPrices { get; set; } = new();
}
