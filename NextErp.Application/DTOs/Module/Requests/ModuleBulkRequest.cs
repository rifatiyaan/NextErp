using NextErp.Domain.Entities;

namespace NextErp.Application.DTOs.Module;

public sealed record ModuleBulkRequest
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? SearchTerm { get; init; }
    public bool? IsActive { get; init; }
    public ModuleType? Type { get; init; }
    public int? ParentId { get; init; }
    public string? SortBy { get; init; }
    public bool SortDescending { get; init; }
}
