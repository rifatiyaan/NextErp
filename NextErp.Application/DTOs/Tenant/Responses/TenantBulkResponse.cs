namespace NextErp.Application.DTOs.Tenant;

public sealed record TenantBulkResponse
{
    public List<TenantResponse> Tenants { get; init; } = new();
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages { get; init; }
}
