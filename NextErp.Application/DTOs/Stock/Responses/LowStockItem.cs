namespace NextErp.Application.DTOs.Stock;

public sealed record LowStockItem
{
    public int ProductVariantId { get; init; }
    public int ProductId { get; init; }
    public string ProductTitle { get; init; } = null!;
    public string ProductCode { get; init; } = null!;
    public string VariantSku { get; init; } = null!;
    public string VariantTitle { get; init; } = null!;
    public decimal AvailableQuantity { get; init; }
    public decimal? ReorderLevel { get; init; }
    public string? UnitOfMeasureAbbreviation { get; init; }
    public string Status { get; init; } = null!;
}
