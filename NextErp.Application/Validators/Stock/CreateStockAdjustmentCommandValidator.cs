using FluentValidation;
using NextErp.Application.Commands;
using NextErp.Domain.Entities;

namespace NextErp.Application.Validators.Stock;

public sealed class CreateStockAdjustmentCommandValidator
    : AbstractValidator<CreateStockAdjustmentCommand>
{
    public CreateStockAdjustmentCommandValidator()
    {
        RuleFor(x => x.ProductVariantId)
            .GreaterThan(0)
            .WithMessage("ProductVariantId must be greater than 0.");

        RuleFor(x => x.Mode)
            .IsInEnum()
            .WithMessage("Mode must be Increase, Decrease, or SetAbsolute.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0m)
            .When(x => x.Mode != StockAdjustmentMode.SetAbsolute)
            .WithMessage("Quantity must be positive for Increase/Decrease.");

        RuleFor(x => x.Quantity)
            .GreaterThanOrEqualTo(0m)
            .When(x => x.Mode == StockAdjustmentMode.SetAbsolute)
            .WithMessage("Quantity must be non-negative for SetAbsolute.");

        RuleFor(x => x.ReasonCode)
            .NotEmpty()
            .WithMessage("ReasonCode is required.")
            .Must(code => StockAdjustmentReason.All.Contains(code))
            .WithMessage("Invalid reason code.");

        RuleFor(x => x.Notes)
            .MaximumLength(500)
            .WithMessage("Notes cannot exceed 500 characters.");
    }
}

