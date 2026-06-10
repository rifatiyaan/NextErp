using FluentValidation;
using NextErp.Application.Commands.Identity;

namespace NextErp.Application.Validators.Identity;

public sealed class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required.")
            .EmailAddress()
            .WithMessage("Email must be a valid email address.")
            .MaximumLength(256);

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required.")
            .MinimumLength(8)
            .WithMessage("Password must be at least 8 characters.")
            .MaximumLength(128);

        RuleFor(x => x.FirstName).MaximumLength(100);
        RuleFor(x => x.LastName).MaximumLength(100);
        RuleFor(x => x.RoleName).MaximumLength(64);

        // Branch must be explicitly chosen by a global caller — per-branch admins
        // fall back to their own branch in the handler, so BranchId may be null
        // in that case. Guid.Empty is NOT rejected here: it's the legitimate id
        // of the single-tenant Main Branch, so requiring "non-empty" would make
        // that branch unselectable.
        When(x => x.CallerIsGlobal, () =>
        {
            RuleFor(x => x.BranchId)
                .Must(b => b.HasValue)
                .WithMessage("BranchId is required when creating a user from the global scope.");
        });
    }
}
