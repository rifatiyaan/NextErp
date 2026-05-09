using NextErp.Domain.Common;

namespace NextErp.Domain.Entities;

[BranchScoped]
public class Notification : IEntity<Guid>, ISoftDeletable
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public Guid TenantId { get; set; }
    public Guid BranchId { get; set; }

    // Target user (null = broadcast to all users in the tenant/branch)
    public Guid? UserId { get; set; }

    // Free-form for routing/filtering on the frontend
    // e.g. "ProductCreated", "SaleCreated", "StockAdjusted", "SystemSettingsUpdated"
    public string Type { get; set; } = null!;

    public string Message { get; set; } = null!;

    // Optional related entity (so frontend can deep-link)
    public string? RelatedEntityType { get; set; }
    public string? RelatedEntityId { get; set; }

    public DateTime? ReadAt { get; set; }
    public DateTime CreatedAt { get; set; }

    // Required by [BranchScoped] global query filter; we keep notifications soft-deletable
    // for parity with other branch-scoped entities (the filter requires IsActive).
    public bool IsActive { get; set; } = true;
}
