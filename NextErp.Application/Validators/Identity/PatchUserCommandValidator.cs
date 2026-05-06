using FluentValidation;
using NextErp.Application.Commands.Identity;

namespace NextErp.Application.Validators.Identity;

public sealed class PatchUserCommandValidator : AbstractValidator<PatchUserCommand>
{
    public PatchUserCommandValidator()
    {
        RuleFor(x => x.UserId).NotEqual(Guid.Empty);

        // BranchId optional; when set, must be non-empty Guid
        RuleFor(x => x.BranchId)
            .Must(b => !b.HasValue || b.Value != Guid.Empty)
            .WithMessage("BranchId must be a valid Guid when set.");

        // RoleName optional; when set, length + charset
        RuleFor(x => x.RoleName)
            .MaximumLength(64)
            .Matches(@"^[\w\- ]+$").When(x => !string.IsNullOrWhiteSpace(x.RoleName))
            .WithMessage("Role name may only contain letters, digits, spaces, dashes and underscores.");
    }
}
