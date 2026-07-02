namespace NextErp.Application.DTOs.Branch;

public sealed record BranchBulkRequest
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? SearchTerm { get; init; }
    public bool? IsActive { get; init; }
    public Guid? TenantId { get; init; }
    public string? SortBy { get; init; }
    public bool SortDescending { get; init; }
}
