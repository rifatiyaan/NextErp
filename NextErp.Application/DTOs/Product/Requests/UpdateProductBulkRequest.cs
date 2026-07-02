namespace NextErp.Application.DTOs.Product;

public sealed record UpdateProductBulkRequest
{
    public List<UpdateProductRequest> Products { get; set; } = new();
}
