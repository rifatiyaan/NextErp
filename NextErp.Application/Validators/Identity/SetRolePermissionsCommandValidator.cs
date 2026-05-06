using FluentValidation;
using NextErp.Application.Commands.Identity;

namespace NextErp.Application.Validators.Identity;

public sealed class SetRolePermissionsCommandValidator : AbstractValidator<SetRolePermissionsCommand>
{
    public SetRolePermissionsCommandValidator()
    {
        RuleFor(x => x.RoleId).NotEqual(Guid.Empty);

        RuleFor(x => x.PermissionKeys)
            .NotNull()
            .WithMessage("PermissionKeys list is required (use empty list to clear).");

        RuleForEach(x => x.PermissionKeys)
            .NotEmpty()
            .MaximumLength(100)
            .Matches(@"^[a-zA-Z0-9_:.\-]+$")
            .WithMessage("Permission keys may only contain letters, digits, colons, dots, dashes and underscores.");
    }
}
