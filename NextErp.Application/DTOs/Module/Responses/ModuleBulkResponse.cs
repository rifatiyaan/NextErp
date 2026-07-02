namespace NextErp.Application.DTOs.Module;

public sealed record ModuleBulkResponse
{
    public List<ModuleResponse> Modules { get; init; } = new();
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages { get; init; }
}
