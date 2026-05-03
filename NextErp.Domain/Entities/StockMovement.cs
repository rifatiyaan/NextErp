using NextErp.Domain.Common;

namespace NextErp.Domain.Entities;

[BranchScoped]
public class StockMovement : ISoftDeletable
{
    public Guid Id { get; set; }

    public Guid StockId { get; set; }
    public Stock Stock { get; set; } = null!;

    public int ProductVariantId { get; set; }
    public ProductVariant ProductVariant { get; set; } = null!;

    public Guid BranchId { get; set; }

    public bool IsActive { get; set; } = true;

    public decimal QuantityChanged { get; set; }

    public decimal PreviousQuantity { get; set; }

    public decimal NewQuantity { get; set; }

    public StockMovementType MovementType { get; set; }

    public Guid ReferenceId { get; set; }

    public string? Reason { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }
}

