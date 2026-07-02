namespace NextErp.Application.DTOs.Product;

public sealed record ProductBulkRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; }
    public int? CategoryId { get; set; }
    public bool? IsActive { get; set; }
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; }
}
