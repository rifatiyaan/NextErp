using NextErp.Domain.Common;

namespace NextErp.Domain.Entities;

[BranchScoped]
public class StockMovement : ISoftDeletable
{
    public Guid Id { get; set; }

    public int ProductVariantId { get; set; }
    public ProductVariant ProductVariant { get; set; } = null!;

    public Guid BranchId { get; set; }

    public bool IsActive { get; set; } = true;

    /// <summary>Signed quantity change (negative reduces on-hand stock, positive increases).</summary>
    public decimal Quantity { get; set; }

    public StockMovementType Type { get; set; }

    public Guid ReferenceId { get; set; }

    public DateTime CreatedAt { get; set; }
}
