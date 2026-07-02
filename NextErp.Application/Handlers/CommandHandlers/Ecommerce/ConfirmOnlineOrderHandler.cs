using MediatR;
using Microsoft.EntityFrameworkCore;
using NextErp.Application.Commands;
using NextErp.Application.Commands.Ecommerce;
using NextErp.Application.Interfaces;
using Entities = NextErp.Domain.Entities;
using SaleDto = NextErp.Application.DTOs.Sale;

namespace NextErp.Application.Handlers.CommandHandlers.Ecommerce;

public class ConfirmOnlineOrderHandler(
    IApplicationDbContext dbContext,
    IMediator mediator,
    IBranchProvider branchProvider)
    : IRequestHandler<ConfirmOnlineOrderCommand, Guid>
{
    public async Task<Guid> Handle(ConfirmOnlineOrderCommand request, CancellationToken cancellationToken = default)
    {
        var order = await dbContext.OnlineOrders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Online order {request.Id} not found.");

        if (order.Status != Entities.OnlineOrderStatus.Pending)
            throw new InvalidOperationException($"Only Pending orders can be confirmed (current: {order.Status}).");

        // The Sale pipeline stamps the operator's current branch; confirming
        // from another branch would move the wrong branch's stock.
        var currentBranch = branchProvider.GetRequiredBranchId();
        if (order.BranchId != currentBranch)
            throw new InvalidOperationException("Switch to the storefront's selling branch to confirm online orders.");

        var party = await dbContext.Parties
            .FirstOrDefaultAsync(p => p.Phone == order.Phone && p.IsActive
                                      && p.PartyType == Entities.PartyType.Customer, cancellationToken);
        if (party is null)
        {
            party = new Entities.Party
            {
                Id = Guid.NewGuid(),
                Title = order.CustomerName,
                Phone = order.Phone,
                Address = order.Address,
                PartyType = Entities.PartyType.Customer,
                TenantId = order.TenantId,
                BranchId = order.BranchId,
                CreatedAt = DateTime.UtcNow,
            };
            dbContext.Parties.Add(party);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        // Snapshot prices are authoritative — the promotion engine is NOT
        // re-run; the customer pays exactly what the store quoted.
        var saleItems = order.Items.Select(i => new SaleDto.SaleItemRequest
        {
            ProductVariantId = i.ProductVariantId,
            Quantity = i.Quantity,
            Price = i.UnitPrice,
            Subtotal = i.LineTotal,
        }).ToList();

        var saleId = await mediator.Send(
            new CreateSaleCommand(party.Id, Discount: 0m, PaymentMethod: null, PaidAmount: null, Items: saleItems),
            cancellationToken);

        order.PartyId = party.Id;
        order.SaleId = saleId;
        order.Status = Entities.OnlineOrderStatus.Confirmed;
        order.ConfirmedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        return saleId;
    }
}
