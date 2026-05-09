namespace NextErp.Application.Interfaces;

public interface INotificationService
{
    /// <summary>
    /// Stages a notification on the change tracker. The caller's SaveChanges flushes it,
    /// so the notification row joins the same transaction as the originating action.
    /// </summary>
    Task RecordAsync(
        string type,
        string title,
        string message,
        string? relatedEntityType = null,
        string? relatedEntityId = null,
        Guid? targetUserId = null,
        CancellationToken cancellationToken = default);
}
