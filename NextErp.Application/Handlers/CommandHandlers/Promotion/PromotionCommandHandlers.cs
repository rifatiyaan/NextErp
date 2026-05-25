using MediatR;
using Microsoft.EntityFrameworkCore;
using NextErp.Application.Commands.Promotion;
using NextErp.Application.DTOs.Promotion;
using NextErp.Application.Interfaces;
using DomainPromotion = NextErp.Domain.Entities.Promotion;
using DomainPromotionConfig = NextErp.Domain.Entities.PromotionConfig;

namespace NextErp.Application.Handlers.CommandHandlers.Promotion;

/// <summary>
/// Promotion CRUD. Promotions are tenant-wide so we always write
/// TenantId = Guid.Empty (single-tenant pet project). FluentValidation
/// runs ahead of these handlers via the validation pipeline behaviour
/// and rejects invalid type/config combos at the API edge.
/// </summary>
public sealed class CreatePromotionHandler(
    IApplicationDbContext db,
    IUserContext userContext)
    : IRequestHandler<CreatePromotionCommand, Guid>
{
    public async Task<Guid> Handle(CreatePromotionCommand request, CancellationToken cancellationToken = default)
    {
        var dto = request.Request;
        var entity = new DomainPromotion
        {
            Id = Guid.NewGuid(),
            Name = dto.Name.Trim(),
            Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim(),
            Type = dto.Type,
            Config = MapConfig(dto.Config),
            IsActive = dto.IsActive,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Priority = dto.Priority,
            Stackable = dto.Stackable,
            TenantId = Guid.Empty,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userContext.UserId,
        };

        db.Promotions.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }

    internal static DomainPromotionConfig MapConfig(PromotionDto.Request.ConfigDto c) => new()
    {
        DiscountAmount = c.DiscountAmount,
        DiscountPercent = c.DiscountPercent,
        MinSubtotal = c.MinSubtotal,
        ScopeProductId = c.ScopeProductId,
        ScopeCategoryId = c.ScopeCategoryId,
        ScopeProductVariantId = c.ScopeProductVariantId,
        // Defensive copy + dedupe for the JSON-stored multi-select lists.
        // Empty lists collapse to null so DB rows stay clean.
        ScopeProductIds = (c.ScopeProductIds != null && c.ScopeProductIds.Count > 0)
            ? c.ScopeProductIds.Distinct().ToList()
            : null,
        ScopeCategoryIds = (c.ScopeCategoryIds != null && c.ScopeCategoryIds.Count > 0)
            ? c.ScopeCategoryIds.Distinct().ToList()
            : null,
        BuyQuantity = c.BuyQuantity,
        GetQuantity = c.GetQuantity,
        GetDiscountPercent = c.GetDiscountPercent,
        BogoProductId = c.BogoProductId,
        BogoVariantId = c.BogoVariantId,
        BuyProductId = c.BuyProductId,
        BuyCategoryId = c.BuyCategoryId,
        GetProductId = c.GetProductId,
        GetCategoryId = c.GetCategoryId,
        MembershipTier = string.IsNullOrWhiteSpace(c.MembershipTier) ? null : c.MembershipTier.Trim(),
    };
}

public sealed class UpdatePromotionHandler(IApplicationDbContext db)
    : IRequestHandler<UpdatePromotionCommand, bool>
{
    public async Task<bool> Handle(UpdatePromotionCommand request, CancellationToken cancellationToken = default)
    {
        var entity = await db.Promotions.FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);
        if (entity == null) return false;

        var dto = request.Request;
        entity.Name = dto.Name.Trim();
        entity.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim();
        entity.Type = dto.Type;
        entity.Config = CreatePromotionHandler.MapConfig(dto.Config);
        entity.IsActive = dto.IsActive;
        entity.StartDate = dto.StartDate;
        entity.EndDate = dto.EndDate;
        entity.Priority = dto.Priority;
        entity.Stackable = dto.Stackable;
        entity.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
        return true;
    }
}

public sealed class DeactivatePromotionHandler(IApplicationDbContext db)
    : IRequestHandler<DeactivatePromotionCommand, bool>
{
    public async Task<bool> Handle(DeactivatePromotionCommand request, CancellationToken cancellationToken = default)
    {
        var entity = await db.Promotions.FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);
        if (entity == null) return false;
        // Soft-delete: keeps historical SaleItem.PromotionId links intact so
        // past sales still report which promotion applied.
        entity.IsActive = false;
        entity.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
