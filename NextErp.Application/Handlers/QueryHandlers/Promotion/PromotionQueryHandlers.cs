using MediatR;
using Microsoft.EntityFrameworkCore;
using NextErp.Application.DTOs.Promotion;
using NextErp.Application.Interfaces;
using NextErp.Application.Mapping;
using NextErp.Application.Queries.Promotion;

namespace NextErp.Application.Handlers.QueryHandlers.Promotion;

public sealed class GetPromotionByIdHandler(IApplicationDbContext db)
    : IRequestHandler<GetPromotionByIdQuery, PromotionResponse?>
{
    public async Task<PromotionResponse?> Handle(GetPromotionByIdQuery request, CancellationToken cancellationToken = default)
    {
        var p = await db.Promotions
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        return p?.ToResponse();
    }
}

public sealed class GetPagedPromotionsHandler(IApplicationDbContext db)
    : IRequestHandler<GetPagedPromotionsQuery, PagedPromotionResponse>
{
    public async Task<PagedPromotionResponse> Handle(GetPagedPromotionsQuery request, CancellationToken cancellationToken = default)
    {
        var page = request.PageIndex < 1 ? 1 : request.PageIndex;
        var pageSize = request.PageSize < 1 ? 50 : request.PageSize;

        var baseQuery = db.Promotions.AsNoTracking();
        var total = await baseQuery.CountAsync(cancellationToken);

        var filtered = baseQuery;
        if (request.Type.HasValue)
            filtered = filtered.Where(p => p.Type == request.Type.Value);
        if (request.OnlyActive == true)
            filtered = filtered.Where(p => p.IsActive);
        if (!string.IsNullOrWhiteSpace(request.SearchText))
        {
            var s = request.SearchText.Trim();
            filtered = filtered.Where(p =>
                p.Name.Contains(s) ||
                (p.Description != null && p.Description.Contains(s)));
        }

        var totalDisplay = await filtered.CountAsync(cancellationToken);

        var rows = await filtered
            .OrderByDescending(p => p.Priority)
            .ThenByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedPromotionResponse
        {
            Total = total,
            TotalDisplay = totalDisplay,
            Data = rows.Select(r => r.ToResponse()).ToList(),
        };
    }
}
