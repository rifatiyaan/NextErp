using MediatR;
using Microsoft.EntityFrameworkCore;
using NextErp.Application.Commands.Ecommerce;
using NextErp.Application.Interfaces;
using Entities = NextErp.Domain.Entities;

namespace NextErp.Application.Handlers.CommandHandlers.Ecommerce;

public class CancelOnlineOrderHandler(IApplicationDbContext dbContext)
    : IRequestHandler<CancelOnlineOrderCommand, Unit>
{
    public async Task<Unit> Handle(CancelOnlineOrderCommand request, CancellationToken cancellationToken = default)
    {
        var order = await dbContext.OnlineOrders
            .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Online order {request.Id} not found.");

        if (order.Status != Entities.OnlineOrderStatus.Pending)
            throw new InvalidOperationException($"Only Pending orders can be cancelled (current: {order.Status}).");
        if (string.IsNullOrWhiteSpace(request.Reason))
            throw new InvalidOperationException("A cancel reason is required.");

        order.Status = Entities.OnlineOrderStatus.Cancelled;
        order.CancelReason = request.Reason.Trim();
        await dbContext.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
