using MediatR;
using Microsoft.EntityFrameworkCore;
using NextErp.Application.Commands.Promotion;
using NextErp.Application.Interfaces;
using NextErp.Application.Mapping;
using DomainPromotionConfig = NextErp.Domain.Entities.PromotionConfig;

namespace NextErp.Application.Handlers.CommandHandlers.Promotion;

public sealed class CreatePromotionHandler(
    IApplicationDbContext db,
    IUserContext userContext)
    : IRequestHandler<CreatePromotionCommand, Guid>
{
    public async Task<Guid> Handle(CreatePromotionCommand request, CancellationToken cancellationToken = default)
    {
        var entity = request.Request.ToEntity();
        entity.Id = Guid.NewGuid();
        entity.Name = entity.Name.Trim();
        entity.Description = string.IsNullOrWhiteSpace(entity.Description) ? null : entity.Description.Trim();
        entity.TenantId = Guid.Empty;
        entity.CreatedAt = DateTime.UtcNow;
        entity.CreatedBy = userContext.UserId;
        NormalizeConfig(entity.Config);

        db.Promotions.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }

    // Mapperly copies config fields straight; business normalization stays
    // here: dedupe multi-select lists, collapse empties to null, trim the tier.
    internal static void NormalizeConfig(DomainPromotionConfig c)
    {
        c.ScopeProductIds = NormalizeList(c.ScopeProductIds);
        c.ScopeCategoryIds = NormalizeList(c.ScopeCategoryIds);
        c.BuyProductIds = NormalizeList(c.BuyProductIds);
        c.BuyCategoryIds = NormalizeList(c.BuyCategoryIds);
        c.GetProductIds = NormalizeList(c.GetProductIds);
        c.MaxRewardQuantity = c.MaxRewardQuantity is > 0 ? c.MaxRewardQuantity : null;
        c.MembershipTier = string.IsNullOrWhiteSpace(c.MembershipTier) ? null : c.MembershipTier.Trim();
    }

    private static List<int>? NormalizeList(List<int>? source) =>
        source is { Count: > 0 } ? source.Distinct().ToList() : null;
}

public sealed class UpdatePromotionHandler(IApplicationDbContext db)
    : IRequestHandler<UpdatePromotionCommand, bool>
{
    public async Task<bool> Handle(UpdatePromotionCommand request, CancellationToken cancellationToken = default)
    {
        var entity = await db.Promotions.FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);
        if (entity == null) return false;

        request.Request.ApplyTo(entity);
        entity.Name = entity.Name.Trim();
        entity.Description = string.IsNullOrWhiteSpace(entity.Description) ? null : entity.Description.Trim();
        entity.UpdatedAt = DateTime.UtcNow;
        CreatePromotionHandler.NormalizeConfig(entity.Config);

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
        // Soft-delete keeps historical SaleItem.PromotionId links intact.
        entity.IsActive = false;
        entity.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
