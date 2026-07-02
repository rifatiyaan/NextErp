using FluentValidation;
using NextErp.Application.Commands.Ecommerce;

namespace NextErp.Application.Validators.Ecommerce;

public class CreateOnlineOrderCommandValidator : AbstractValidator<CreateOnlineOrderCommand>
{
    public CreateOnlineOrderCommandValidator()
    {
        RuleFor(c => c.CustomerName).NotEmpty().MaximumLength(200);
        RuleFor(c => c.Phone).NotEmpty().MaximumLength(32)
            .Matches(@"^[0-9+\-\s()]{6,}$").WithMessage("Phone number looks invalid.");
        RuleFor(c => c.Address).NotEmpty().MaximumLength(1000);
        RuleFor(c => c.Note).MaximumLength(1000);
        RuleFor(c => c.Items).NotEmpty().WithMessage("The cart is empty.");
        RuleForEach(c => c.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductVariantId).GreaterThan(0);
            item.RuleFor(i => i.Quantity).InclusiveBetween(1, 99);
        });
    }
}
