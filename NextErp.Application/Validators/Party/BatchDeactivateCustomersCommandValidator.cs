using FluentValidation;
using NextErp.Application.Commands;

namespace NextErp.Application.Validators.Party;

public sealed class BatchDeactivateCustomersCommandValidator
    : AbstractValidator<BatchDeactivateCustomersCommand>
{
    public BatchDeactivateCustomersCommandValidator()
    {
        RuleFor(x => x.Ids)
            .NotNull()
            .NotEmpty().WithMessage("At least one id is required.")
            .Must(ids => ids.Count <= 100).WithMessage("Maximum 100 items per batch.");
    }
}
