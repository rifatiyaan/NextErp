namespace NextErp.Application.DTOs.Module;

public sealed record CreateBulkModulesResponse
{
    // set: the bulk handler tallies these incrementally as it processes rows.
    public List<CreateModuleHierarchicalResponse> Modules { get; init; } = new();
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public List<string> Errors { get; init; } = new();
}
