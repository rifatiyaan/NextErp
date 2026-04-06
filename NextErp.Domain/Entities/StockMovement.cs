using NextErp.Domain.Common;

namespace NextErp.Domain.Entities;

[BranchScoped]
public class StockMovement : ISoftDeletable
{
    public Guid Id { get; set; }

    /// <summary>Stock row that was affected (ledger line).</summary>
    public Guid StockId { get; set; }
    public Stock Stock { get; set; } = null!;

    public int ProductVariantId { get; set; }
    public ProductVariant ProductVariant { get; set; } = null!;

    public Guid BranchId { get; set; }

    public bool IsActive { get; set; } = true;

    /// <summary>Delta applied (negative for outbound).</summary>
    public decimal QuantityChanged { get; set; }

    public decimal PreviousQuantity { get; set; }

    public decimal NewQuantity { get; set; }

    public StockMovementType MovementType { get; set; }

    /// <summary>Source document id when applicable (e.g. sale, purchase).</summary>
    public Guid ReferenceId { get; set; }

    public DateTime CreatedAt { get; set; }
}
