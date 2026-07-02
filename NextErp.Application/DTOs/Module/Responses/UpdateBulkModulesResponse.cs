namespace NextErp.Application.DTOs.Module;

public sealed record UpdateBulkModulesResponse
{
    public List<UpdateModuleResponse> Modules { get; init; } = new();
    public int SuccessCount { get; init; }
    public int FailureCount { get; init; }
    public List<string> Errors { get; init; } = new();
}
