using MediatR;
using NextErp.Application.DTOs.Notification;

namespace NextErp.Application.Queries.Notifications;

/// <summary>
/// Lists notifications for the authenticated user, filtered server-side.
/// </summary>
/// <param name="Page">1-based page index.</param>
/// <param name="PageSize">Items per page. Clamped to [1, 100] in the handler.</param>
/// <param name="UnreadOnly">If true, only rows where <c>ReadAt</c> is null.</param>
/// <param name="Type">
/// Case-insensitive prefix match against <c>Notification.Type</c>. Pass a
/// category like "Product" to capture both "ProductCreated" and
/// "ProductUpdated", or a full type to filter exactly. Null/empty returns
/// every type.
/// </param>
public record GetNotificationsQuery(
    int Page = 1,
    int PageSize = 20,
    bool UnreadOnly = false,
    string? Type = null
) : IRequest<NotificationListResponse>;
