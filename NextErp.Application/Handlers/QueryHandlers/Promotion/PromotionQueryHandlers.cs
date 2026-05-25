using MediatR;
using Microsoft.EntityFrameworkCore;
using NextErp.Application.DTOs.Promotion;
using NextErp.Application.Interfaces;
using NextErp.Application.Queries.Promotion;
using DomainPromotionConfig = NextErp.Domain.Entities.PromotionConfig;

namespace NextErp.Application.Handlers.QueryHandlers.Promotion;

public sealed class GetPromotionByIdHandler(IApplicationDbContext db)
    : IRequestHandler<GetPromotionByIdQuery, PromotionDto.Response.Single?>
{
    public async Task<PromotionDto.Response.Single?> Handle(GetPromotionByIdQuery request, CancellationToken cancellationToken = default)
    {
        var p = await db.Promotions
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        return p == null ? null : MapToDto(p);
    }

    internal static PromotionDto.Response.Single MapToDto(NextErp.Domain.Entities.Promotion p) => new()
    {
        Id = p.Id,
        Name = p.Name,
        Description = p.Description,
        Type = p.Type,
        Config = MapConfig(p.Config),
        IsActive = p.IsActive,
        StartDate = p.StartDate,
        EndDate = p.EndDate,
        Priority = p.Priority,
        Stackable = p.Stackable,
        CreatedAt = p.CreatedAt,
        UpdatedAt = p.UpdatedAt,
    };

    private static PromotionDto.Request.ConfigDto MapConfig(DomainPromotionConfig c) => new()
    {
        DiscountAmount = c.DiscountAmount,
        DiscountPercent = c.DiscountPercent,
        MinSubtotal = c.MinSubtotal,
        ScopeProductId = c.ScopeProductId,
        ScopeCategoryId = c.ScopeCategoryId,
        ScopeProductVariantId = c.ScopeProductVariantId,
        // Multi-select arrays — emit defensive copies so any later
        // mutation of the response DTO can't reach back into the entity.
        ScopeProductIds = c.ScopeProductIds?.ToList(),
        ScopeCategoryIds = c.ScopeCategoryIds?.ToList(),
        BuyQuantity = c.BuyQuantity,
        GetQuantity = c.GetQuantity,
        GetDiscountPercent = c.GetDiscountPercent,
        BogoProductId = c.BogoProductId,
        BogoVariantId = c.BogoVariantId,
        BuyProductId = c.BuyProductId,
        BuyCategoryId = c.BuyCategoryId,
        GetProductId = c.GetProductId,
        GetCategoryId = c.GetCategoryId,
        MembershipTier = c.MembershipTier,
    };
}

public sealed class GetPagedPromotionsHandler(IApplicationDbContext db)
    : IRequestHandler<GetPagedPromotionsQuery, PromotionDto.Response.Paged>
{
    public async Task<PromotionDto.Response.Paged> Handle(GetPagedPromotionsQuery request, CancellationToken cancellationToken = default)
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

        return new PromotionDto.Response.Paged
        {
            Total = total,
            TotalDisplay = totalDisplay,
            Data = rows.Select(GetPromotionByIdHandler.MapToDto).ToList(),
        };
    }
}
