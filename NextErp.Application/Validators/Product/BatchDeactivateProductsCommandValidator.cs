using FluentValidation;
using NextErp.Application.Commands;

namespace NextErp.Application.Validators.Product;

public sealed class BatchDeactivateProductsCommandValidator
    : AbstractValidator<BatchDeactivateProductsCommand>
{
    public BatchDeactivateProductsCommandValidator()
    {
        RuleFor(x => x.Ids)
            .NotNull()
            .NotEmpty().WithMessage("At least one id is required.")
            .Must(ids => ids.Count <= 100).WithMessage("Maximum 100 items per batch.");
    }
}
