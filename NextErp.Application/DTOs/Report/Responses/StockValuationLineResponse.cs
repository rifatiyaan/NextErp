namespace NextErp.Application.DTOs.Report;

public sealed record StockValuationLineResponse
{
    public int ProductId { get; init; }
    public string ProductTitle { get; init; } = null!;
    public string? VariantSku { get; init; }
    public string? Category { get; init; }
    public decimal Quantity { get; init; }
    public decimal UnitCost { get; init; }
    public decimal Value { get; init; }
}
