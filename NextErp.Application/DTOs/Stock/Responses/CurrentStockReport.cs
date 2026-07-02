namespace NextErp.Application.DTOs.Stock;

public sealed record CurrentStockReport
{
    public List<StockResponse> Stocks { get; init; } = new();
    public int TotalVariants { get; init; }
    public decimal TotalQuantity { get; init; }
}
