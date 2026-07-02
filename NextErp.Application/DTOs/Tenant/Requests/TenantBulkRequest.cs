namespace NextErp.Application.DTOs.Tenant;

public sealed record TenantBulkRequest
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? SearchTerm { get; init; }
    public bool? IsActive { get; init; }
    public string? SortBy { get; init; }
    public bool SortDescending { get; init; }
}
