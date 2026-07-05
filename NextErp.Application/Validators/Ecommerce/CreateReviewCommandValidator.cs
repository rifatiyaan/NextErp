using FluentValidation;
using NextErp.Application.Commands.Ecommerce;

namespace NextErp.Application.Validators.Ecommerce;

public class CreateReviewCommandValidator : AbstractValidator<CreateReviewCommand>
{
    public CreateReviewCommandValidator()
    {
        RuleFor(c => c.AuthorName).NotEmpty().MaximumLength(100);
        RuleFor(c => c.Rating).InclusiveBetween(1, 5);
        RuleFor(c => c.Text).NotEmpty().MaximumLength(2000);
    }
}
