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

        // Branch is required for global callers — per-branch admins fall back to
        // their own branch in the handler, so BranchId may be null in that case.
        When(x => x.CallerIsGlobal, () =>
        {
            RuleFor(x => x.BranchId)
                .NotEqual(Guid.Empty)
                .WithMessage("BranchId is required when creating a user from the global scope.")
                .Must(b => b.HasValue && b.Value != Guid.Empty)
                .WithMessage("BranchId must be a valid Guid.");
        });
    }
}
