namespace NextErp.Application.DTOs.Report;

public sealed record StockValuationResponse
{
    public DateTime AsOf { get; init; }
    public int ProductCount { get; init; }
    public decimal TotalQuantity { get; init; }
    public decimal TotalValue { get; init; }
    public List<StockValuationLineResponse> Lines { get; init; } = new();
}
