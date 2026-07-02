namespace NextErp.Application.DTOs.Module;

public sealed record CreateBulkModulesRequest
{
    public List<CreateModuleHierarchicalRequest> Modules { get; init; } = new();
}
