using NextErp.Application.Interfaces;
using NextErp.Domain.Entities;

namespace NextErp.Application.Services;

/// <summary>
/// Records notifications by staging entities on the DbContext change tracker.
/// The caller's SaveChangesAsync persists them as part of the orchestrating transaction.
/// </summary>
public sealed class NotificationService(
    IApplicationDbContext dbContext,
    IBranchProvider branchProvider,
    IUserContext userContext) : INotificationService
{
    public Task RecordAsync(
        string type,
        string title,
        string message,
        string? relatedEntityType = null,
        string? relatedEntityId = null,
        Guid? targetUserId = null,
        CancellationToken cancellationToken = default)
    {
        // Branch context is required for notifications to be visible to users via the
        // global [BranchScoped] filter. Fallback to Guid.Empty for system-level events
        // (e.g. SystemSettings updates which are tenant-global).
        var branchId = branchProvider.GetBranchId() ?? Guid.Empty;

        // TenantId resolution: if we ever introduce a per-tenant context, this is
        // the place to read it. For now the system has a single tenant scope so
        // Guid.Empty matches the SystemSettings convention.
        var tenantId = Guid.Empty;

        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            Title = title,
            TenantId = tenantId,
            BranchId = branchId,
            UserId = targetUserId,
            Type = type,
            Message = message,
            RelatedEntityType = relatedEntityType,
            RelatedEntityId = relatedEntityId,
            ReadAt = null,
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
        };

        dbContext.Notifications.Add(notification);
        // No SaveChanges — caller flushes.
        _ = userContext; // reserved for future use (e.g. fan-out to admin users).
        return Task.CompletedTask;
    }
}
