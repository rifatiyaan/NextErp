using MediatR;
using Microsoft.EntityFrameworkCore;
using NextErp.Application.Commands.Notifications;
using NextErp.Application.Interfaces;

namespace NextErp.Application.Handlers.CommandHandlers.Notifications;

public sealed class MarkAllNotificationsReadHandler(
    IApplicationDbContext dbContext,
    IUserContext userContext)
    : IRequestHandler<MarkAllNotificationsReadCommand, Unit>
{
    public async Task<Unit> Handle(
        MarkAllNotificationsReadCommand request,
        CancellationToken cancellationToken = default)
    {
        var userId = userContext.UserId;
        var now = DateTime.UtcNow;

        // Branch-scoped via global filter; mark every visible-to-user unread row.
        var unread = await dbContext.Notifications
            .Where(n => n.ReadAt == null
                        && (n.UserId == null || n.UserId == userId))
            .ToListAsync(cancellationToken);

        if (unread.Count == 0)
            return Unit.Value;

        foreach (var n in unread)
            n.ReadAt = now;

        await dbContext.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
