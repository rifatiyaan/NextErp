using NextErp.Domain.Common;

namespace NextErp.Domain.Entities;

/// <summary>
/// Outbound return of received goods to the original supplier. Mirrors
/// <see cref="SaleReturn"/> but stock moves in the opposite direction —
/// the handler issues a negative-delta <see cref="StockMovement"/> with
/// <see cref="StockMovementType.Return"/> so available quantity drops.
/// Settled at create time; no Draft state.
/// </summary>
[BranchScoped]
public class PurchaseReturn : IEntity<Guid>, ISoftDeletable
{
    public Guid Id { get; set; }

    /// <summary>"RET-P-YYYYMMDD-XXXXXXXX" generated server-side.</summary>
    public string ReturnNumber { get; set; } = null!;

    /// <summary>
    /// IEntity contract requirement; mirrored from <see cref="ReturnNumber"/>.
    /// </summary>
    public string Title
    {
        get => ReturnNumber;
        set => ReturnNumber = value;
    }

    public Guid PurchaseId { get; set; }
    public Purchase Purchase { get; set; } = null!;

    public DateTime ReturnDate { get; set; }

    public string? Reason { get; set; }
    public string? Notes { get; set; }

    /// <summary>Sum of (Quantity × UnitPrice) across all <see cref="Items"/>.</summary>
    public decimal TotalAmount { get; set; }

    public ICollection<PurchaseReturnItem> Items { get; set; } = new List<PurchaseReturnItem>();

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? CreatedBy { get; set; }

    public Guid TenantId { get; set; }
    public Guid BranchId { get; set; }
}
