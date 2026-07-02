using MediatR;
using Microsoft.EntityFrameworkCore;
using NextErp.Application.DTOs.Ecommerce;
using NextErp.Application.Interfaces;
using NextErp.Application.Queries.Ecommerce;
using NextErp.Domain.Entities;

namespace NextErp.Application.Handlers.QueryHandlers.Ecommerce;

public class GetPagedOnlineOrdersHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetPagedOnlineOrdersQuery, PagedOnlineOrdersResponse>
{
    public async Task<PagedOnlineOrdersResponse> Handle(GetPagedOnlineOrdersQuery request, CancellationToken cancellationToken = default)
    {
        var query = dbContext.OnlineOrders.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(request.Status)
            && Enum.TryParse<OnlineOrderStatus>(request.Status, ignoreCase: true, out var status))
        {
            query = query.Where(o => o.Status == status);
        }

        var total = await query.CountAsync(cancellationToken);
        var pageIndex = Math.Max(1, request.PageIndex);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var data = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .Select(o => new OnlineOrderRow(
                o.Id, o.OrderNumber, o.CustomerName, o.Phone,
                o.Items.Count, o.Items.Sum(i => i.LineTotal), o.DeliveryFee,
                o.Status.ToString(), o.CreatedAt))
            .ToListAsync(cancellationToken);

        return new PagedOnlineOrdersResponse(total, data);
    }
}

public class GetOnlineOrderByIdHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetOnlineOrderByIdQuery, OnlineOrderDetailResponse?>
{
    public async Task<OnlineOrderDetailResponse?> Handle(GetOnlineOrderByIdQuery request, CancellationToken cancellationToken = default)
    {
        return await dbContext.OnlineOrders
            .AsNoTracking()
            .Where(o => o.Id == request.Id)
            .Select(o => new OnlineOrderDetailResponse(
                o.Id, o.OrderNumber, o.CustomerName, o.Phone, o.Address, o.Note,
                o.Status.ToString(), o.CancelReason, o.DeliveryFee,
                o.PartyId, o.SaleId, o.CreatedAt, o.ConfirmedAt,
                o.Items.Select(i => new OnlineOrderItemRow(
                    i.ProductTitle, i.Sku, i.UnitPrice, i.Quantity, i.LineTotal)).ToList()))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
