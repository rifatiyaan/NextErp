namespace NextErp.Application.DTOs.Stock;

public sealed record PagedAdjustments
{
    public IReadOnlyList<AdjustmentLine> Items { get; init; } = Array.Empty<AdjustmentLine>();
    public int Total { get; init; }
    public int PageIndex { get; init; }
    public int PageSize { get; init; }
}
