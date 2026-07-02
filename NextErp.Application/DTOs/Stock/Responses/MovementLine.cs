namespace NextErp.Application.DTOs.Stock;

public sealed record MovementLine
{
    public Guid Id { get; init; }
    public Guid StockId { get; init; }
    public int ProductVariantId { get; init; }
    public Guid BranchId { get; init; }
    public decimal QuantityChanged { get; init; }
    public decimal PreviousQuantity { get; init; }
    public decimal NewQuantity { get; init; }
    public string MovementType { get; init; } = null!;
    public Guid ReferenceId { get; init; }
    public string? Reason { get; init; }
    public string? Notes { get; init; }
    public DateTime CreatedAt { get; init; }
}
