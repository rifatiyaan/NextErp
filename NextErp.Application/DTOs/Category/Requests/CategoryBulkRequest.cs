namespace NextErp.Application.DTOs.Category;

public sealed record CategoryBulkRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; }
    public bool? IsActive { get; set; }
    public int? ParentId { get; set; }
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; }
}
