namespace NextErp.Application.DTOs.Stock;

public sealed record StockResponse
{
    public Guid Id { get; init; }
    public int ProductVariantId { get; init; }
    public int ProductId { get; init; }
    public string ProductTitle { get; init; } = null!;
    public string ProductCode { get; init; } = null!;
    public string VariantSku { get; init; } = null!;
    public string VariantTitle { get; init; } = null!;
    public decimal AvailableQuantity { get; init; }
    public decimal? ReorderLevel { get; init; }
    public int? UnitOfMeasureId { get; init; }
    public string? UnitOfMeasureAbbreviation { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public Guid TenantId { get; init; }
    public Guid BranchId { get; init; }
}
