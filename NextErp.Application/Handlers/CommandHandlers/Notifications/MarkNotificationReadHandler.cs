using MediatR;
using Microsoft.EntityFrameworkCore;
using NextErp.Application.Commands.Notifications;
using NextErp.Application.Interfaces;

namespace NextErp.Application.Handlers.CommandHandlers.Notifications;

public sealed class MarkNotificationReadHandler(
    IApplicationDbContext dbContext,
    IUserContext userContext)
    : IRequestHandler<MarkNotificationReadCommand, Unit>
{
    public async Task<Unit> Handle(
        MarkNotificationReadCommand request,
        CancellationToken cancellationToken = default)
    {
        var userId = userContext.UserId;

        // Only allow marking notifications visible to the current user
        // (targeted at them or broadcast within their branch).
        var notification = await dbContext.Notifications
            .Where(n => n.Id == request.NotificationId
                        && (n.UserId == null || n.UserId == userId))
            .FirstOrDefaultAsync(cancellationToken);

        if (notification == null)
            return Unit.Value;

        if (notification.ReadAt == null)
        {
            notification.ReadAt = DateTime.UtcNow;
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return Unit.Value;
    }
}
