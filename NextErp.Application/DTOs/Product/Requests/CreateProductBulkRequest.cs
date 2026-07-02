namespace NextErp.Application.DTOs.Product;

public sealed record CreateProductBulkRequest
{
    public List<CreateProductRequest> Products { get; set; } = new();
}
