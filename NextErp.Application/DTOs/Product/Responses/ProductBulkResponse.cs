namespace NextErp.Application.DTOs.Product;

public sealed record ProductBulkResponse
{
    public List<ProductResponse> Products { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}
