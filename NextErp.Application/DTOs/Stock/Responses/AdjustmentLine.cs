namespace NextErp.Application.DTOs.Stock;

public sealed record AdjustmentLine
{
    public Guid Id { get; init; }
    public int ProductVariantId { get; init; }
    public string VariantSku { get; init; } = null!;
    public string ProductTitle { get; init; } = null!;
    public decimal QuantityChanged { get; init; }
    public decimal PreviousQuantity { get; init; }
    public decimal NewQuantity { get; init; }
    public string ReasonCode { get; init; } = null!;
    public string? Notes { get; init; }
    public DateTime CreatedAt { get; init; }
}
