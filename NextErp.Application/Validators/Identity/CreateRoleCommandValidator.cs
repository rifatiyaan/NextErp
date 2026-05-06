using FluentValidation;
using NextErp.Application.Commands.Identity;

namespace NextErp.Application.Validators.Identity;

public sealed class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
{
    public CreateRoleCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Role name is required.")
            .MinimumLength(2)
            .WithMessage("Role name must be at least 2 characters.")
            .MaximumLength(64)
            .WithMessage("Role name cannot exceed 64 characters.")
            .Matches(@"^[\w\- ]+$")
            .WithMessage("Role name may only contain letters, digits, spaces, dashes and underscores.");
    }
}
