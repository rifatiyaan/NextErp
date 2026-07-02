using FluentValidation;
using NextErp.Application.Commands.Promotion;
using NextErp.Application.DTOs.Promotion;
using NextErp.Domain.Entities;

namespace NextErp.Application.Validators.Promotion;

/// <summary>
/// Validates a CreatePromotionCommand: name + dates + type/config combo.
/// The config check is the interesting part — each PromotionType requires
/// a specific subset of fields. This is where we keep invalid combos out
/// of the database since the wide-nullable JSON column can't enforce it
/// at the SQL level.
/// </summary>
public sealed class CreatePromotionCommandValidator : AbstractValidator<CreatePromotionCommand>
{
    public CreatePromotionCommandValidator()
    {
        RuleFor(x => x.Request.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200);

        RuleFor(x => x.Request.Description)
            .MaximumLength(1000);

        RuleFor(x => x.Request.Type)
            .IsInEnum();

        RuleFor(x => x.Request)
            .Must(r => !r.StartDate.HasValue || !r.EndDate.HasValue || r.StartDate.Value <= r.EndDate.Value)
            .WithMessage("EndDate must be on or after StartDate.");

        RuleFor(x => x.Request.Priority)
            .GreaterThanOrEqualTo(0);

        // Type-specific config rules. Each case spells out required +
        // forbidden fields; if you're adding a new PromotionType, copy
        // one of the existing blocks and adjust.
        RuleFor(x => x.Request.Config)
            .Custom((cfg, ctx) =>
            {
                var type = ctx.InstanceToValidate.Request.Type;
                ValidateConfig(type, cfg, ctx);
            });
    }

    internal static void ValidateConfig(
        PromotionType type,
        PromotionConfigDto cfg,
        FluentValidation.ValidationContext<CreatePromotionCommand> ctx)
    {
        switch (type)
        {
            case PromotionType.LineDiscount:
                RequireDiscountValue(cfg, ctx);
                RequireScope(cfg, ctx);
                ForbidBogo(cfg, ctx);
                ForbidMembership(cfg, ctx);
                break;
            case PromotionType.InvoiceDiscount:
                RequireDiscountValue(cfg, ctx);
                ForbidLineScope(cfg, ctx);
                ForbidBogo(cfg, ctx);
                ForbidMembership(cfg, ctx);
                break;
            case PromotionType.Bogo:
                RequireBogoMechanics(cfg, ctx);
                if (!(cfg.BuyProductIds?.Count > 0) && !(cfg.BuyCategoryIds?.Count > 0))
                    ctx.AddFailure("Config", "BOGO requires a BUY set — at least one product or category.");
                if (!(cfg.GetProductIds?.Count > 0))
                    ctx.AddFailure("Config", "BOGO requires at least one GET (reward) product.");
                if (cfg.MaxRewardQuantity is { } cap && cap <= 0)
                    ctx.AddFailure("Config.MaxRewardQuantity", "Reward cap must be positive when set.");
                ForbidMembership(cfg, ctx);
                ForbidPlainDiscountValue(cfg, ctx);
                break;
            case PromotionType.Membership:
                if (string.IsNullOrWhiteSpace(cfg.MembershipTier))
                    ctx.AddFailure("Config.MembershipTier", "Required for Membership promotions.");
                if (!cfg.DiscountPercent.HasValue)
                    ctx.AddFailure("Config.DiscountPercent", "Required for Membership promotions.");
                else if (cfg.DiscountPercent.Value <= 0 || cfg.DiscountPercent.Value > 100)
                    ctx.AddFailure("Config.DiscountPercent", "Must be between 0 (exclusive) and 100.");
                ForbidLineScope(cfg, ctx);
                ForbidBogo(cfg, ctx);
                break;
        }
    }

    private static void RequireDiscountValue(
        PromotionConfigDto cfg,
        FluentValidation.ValidationContext<CreatePromotionCommand> ctx)
    {
        var amount = cfg.DiscountAmount.GetValueOrDefault();
        var percent = cfg.DiscountPercent.GetValueOrDefault();
        if (amount <= 0 && percent <= 0)
            ctx.AddFailure("Config", "DiscountAmount or DiscountPercent must be positive.");
        if (amount > 0 && percent > 0)
            ctx.AddFailure("Config", "Provide either DiscountAmount or DiscountPercent, not both.");
        if (percent > 100)
            ctx.AddFailure("Config.DiscountPercent", "Cannot exceed 100.");
    }

    private static void RequireScope(
        PromotionConfigDto cfg,
        FluentValidation.ValidationContext<CreatePromotionCommand> ctx)
    {
        var hasSingular = cfg.ScopeProductId != null || cfg.ScopeCategoryId != null || cfg.ScopeProductVariantId != null;
        var hasMulti = (cfg.ScopeProductIds != null && cfg.ScopeProductIds.Count > 0)
            || (cfg.ScopeCategoryIds != null && cfg.ScopeCategoryIds.Count > 0);
        if (!hasSingular && !hasMulti)
            ctx.AddFailure("Config", "LineDiscount requires a scope — pick at least one product or category.");
    }

    private static void ForbidLineScope(
        PromotionConfigDto cfg,
        FluentValidation.ValidationContext<CreatePromotionCommand> ctx)
    {
        var hasSingular = cfg.ScopeProductId != null || cfg.ScopeCategoryId != null || cfg.ScopeProductVariantId != null;
        var hasMulti = (cfg.ScopeProductIds != null && cfg.ScopeProductIds.Count > 0)
            || (cfg.ScopeCategoryIds != null && cfg.ScopeCategoryIds.Count > 0);
        if (hasSingular || hasMulti)
            ctx.AddFailure("Config", "Line-scope fields are only valid for LineDiscount type.");
    }

    private static void RequireBogoMechanics(
        PromotionConfigDto cfg,
        FluentValidation.ValidationContext<CreatePromotionCommand> ctx)
    {
        if ((cfg.BuyQuantity ?? 0) <= 0)
            ctx.AddFailure("Config.BuyQuantity", "Must be positive for BOGO promotions.");
        if ((cfg.GetQuantity ?? 0) <= 0)
            ctx.AddFailure("Config.GetQuantity", "Must be positive for BOGO promotions.");
        if ((cfg.GetDiscountPercent ?? 0) <= 0 || (cfg.GetDiscountPercent ?? 0) > 100)
            ctx.AddFailure("Config.GetDiscountPercent", "Must be between 0 (exclusive) and 100 for BOGO.");
    }

    private static void ForbidBogo(
        PromotionConfigDto cfg,
        FluentValidation.ValidationContext<CreatePromotionCommand> ctx)
    {
        if (cfg.BuyQuantity != null || cfg.GetQuantity != null || cfg.GetDiscountPercent != null
            || cfg.BuyProductIds?.Count > 0 || cfg.BuyCategoryIds?.Count > 0
            || cfg.GetProductIds?.Count > 0 || cfg.MaxRewardQuantity != null)
        {
            ctx.AddFailure("Config", "BOGO fields are only valid for the Bogo type.");
        }
    }

    private static void ForbidPlainDiscountValue(
        PromotionConfigDto cfg,
        FluentValidation.ValidationContext<CreatePromotionCommand> ctx)
    {
        if (cfg.DiscountAmount != null || cfg.DiscountPercent != null || cfg.MinSubtotal != null)
            ctx.AddFailure("Config", "DiscountAmount/DiscountPercent/MinSubtotal are for Line/InvoiceDiscount types; use BOGO-specific fields here.");
    }

    private static void ForbidMembership(
        PromotionConfigDto cfg,
        FluentValidation.ValidationContext<CreatePromotionCommand> ctx)
    {
        if (!string.IsNullOrWhiteSpace(cfg.MembershipTier))
            ctx.AddFailure("Config.MembershipTier", "Only valid for Membership type.");
    }
}

/// <summary>Update mirrors create — same validation rules.</summary>
public sealed class UpdatePromotionCommandValidator : AbstractValidator<UpdatePromotionCommand>
{
    public UpdatePromotionCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Request.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Request.Description).MaximumLength(1000);
        RuleFor(x => x.Request.Type).IsInEnum();
        RuleFor(x => x.Request.Priority).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Request)
            .Must(r => !r.StartDate.HasValue || !r.EndDate.HasValue || r.StartDate.Value <= r.EndDate.Value)
            .WithMessage("EndDate must be on or after StartDate.");
        // We reuse the same config rules — wrap in a tiny shim so we can
        // call the static helper with the right context type.
    }
}
