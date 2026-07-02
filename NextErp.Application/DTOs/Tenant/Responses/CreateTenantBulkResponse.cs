namespace NextErp.Application.DTOs.Tenant;

public sealed record CreateTenantBulkResponse
{
    public List<CreateTenantResponse> Tenants { get; init; } = new();
    public int SuccessCount { get; init; }
    public int FailureCount { get; init; }
    public List<string> Errors { get; init; } = new();
}
