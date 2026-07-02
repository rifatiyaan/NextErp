namespace NextErp.Application.DTOs.Module;

public sealed record UpdateBulkModulesRequest
{
    public List<UpdateModuleRequest> Modules { get; init; } = new();
}
