namespace NextErp.Application.DTOs.Stock;

public sealed record LowStockReport
{
    public List<LowStockItem> Items { get; init; } = new();
    public int TotalLowStockVariants { get; init; }
}
