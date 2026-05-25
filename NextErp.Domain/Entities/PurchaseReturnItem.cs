namespace NextErp.Domain.Entities;

/// <summary>
/// One returned line on a <see cref="PurchaseReturn"/>. We anchor to the
/// original <see cref="PurchaseItemId"/> so a partial return knows which
/// purchase line it's against; matters when the same variant was bought
/// on two separate receipts at different unit costs.
/// </summary>
public class PurchaseReturnItem : IEntity<Guid>
{
    public Guid Id { get; set; }

    /// <summary>IEntity contract requirement.</summary>
    public string Title { get; set; } = string.Empty;

    public Guid PurchaseReturnId { get; set; }
    public PurchaseReturn PurchaseReturn { get; set; } = null!;

    public Guid PurchaseItemId { get; set; }
    public PurchaseItem PurchaseItem { get; set; } = null!;

    public int ProductVariantId { get; set; }
    public ProductVariant ProductVariant { get; set; } = null!;

    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Subtotal => Quantity * UnitPrice;

    public string? ConditionNote { get; set; }

    public DateTime CreatedAt { get; set; }
    public Guid TenantId { get; set; }
}
