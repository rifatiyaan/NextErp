using MediatR;
using Microsoft.EntityFrameworkCore;
using NextErp.Application.Interfaces;
using NextErp.Application.Queries.Notifications;
using NextErp.Application.DTOs.Notification;

namespace NextErp.Application.Handlers.QueryHandlers.Notifications;

public sealed class GetNotificationsHandler(
    IApplicationDbContext dbContext,
    IUserContext userContext)
    : IRequestHandler<GetNotificationsQuery, NotificationListResponse>
{
    public async Task<NotificationListResponse> Handle(
        GetNotificationsQuery request,
        CancellationToken cancellationToken = default)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize is < 1 or > 100 ? 20 : request.PageSize;

        var userId = userContext.UserId;

        // Branch-scoped via global filter; user filter: targeted at me OR broadcast (UserId == null).
        var baseQuery = dbContext.Notifications
            .AsNoTracking()
            .Where(n => n.UserId == null || n.UserId == userId);

        var filteredQuery = request.UnreadOnly
            ? baseQuery.Where(n => n.ReadAt == null)
            : baseQuery;

        // Server-side Type filter. Prefix match so a single dropdown entry
        // ("Product") covers all sub-types ("ProductCreated", "ProductUpdated").
        // EF.Functions.Like translates to a SQL LIKE which the provider can
        // index-scan; StartsWith(...) would be equivalent but Like makes the
        // intent and the case-insensitive collation behavior explicit.
        if (!string.IsNullOrWhiteSpace(request.Type))
        {
            var pattern = request.Type.Trim() + "%";
            filteredQuery = filteredQuery.Where(n => EF.Functions.Like(n.Type, pattern));
        }

        var total = await filteredQuery.CountAsync(cancellationToken);

        // Unread count is independent of the current filter — always represents user's
        // total unread count so the bell badge is consistent.
        var unreadCount = await baseQuery
            .Where(n => n.ReadAt == null)
            .CountAsync(cancellationToken);

        var items = await filteredQuery
            .OrderByDescending(n => n.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(n => new NotificationResponse
            {
                Id = n.Id,
                Title = n.Title,
                Type = n.Type,
                Message = n.Message,
                RelatedEntityType = n.RelatedEntityType,
                RelatedEntityId = n.RelatedEntityId,
                ReadAt = n.ReadAt,
                CreatedAt = n.CreatedAt,
            })
            .ToListAsync(cancellationToken);

        return new NotificationListResponse
        {
            Items = items,
            UnreadCount = unreadCount,
            Total = total,
            Page = page,
            PageSize = pageSize,
        };
    }
}
