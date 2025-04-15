namespace TSWMS.ProductService.Shared.Models.Requests;

public class BatchProductPriceRequest
{
    public List<Guid> ProductIds { get; set; } = new();
}
