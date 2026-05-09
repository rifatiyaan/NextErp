using MediatR;
using NextErp.Application.Common.Interfaces;

namespace NextErp.Application.Commands.Notifications;

public record MarkNotificationReadCommand(Guid NotificationId)
    : IRequest<Unit>, ITransactionalRequest;

public record MarkAllNotificationsReadCommand()
    : IRequest<Unit>, ITransactionalRequest;
