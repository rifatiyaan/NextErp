namespace NextErp.Application.DTOs.Product;

public sealed record CreateProductBulkResponse
{
    public List<CreateProductResponse> Products { get; set; } = new();
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public List<string> Errors { get; set; } = new();
}
