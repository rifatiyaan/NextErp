using MediatR;

namespace NextErp.Application.Commands.Identity
{
    /// <summary>
    /// Replaces all permission keys for a role in one atomic write.
    /// Passing an empty list clears all permissions for that role.
    /// </summary>
    public record SetRolePermissionsCommand(
        Guid RoleId,
        IReadOnlyList<string> PermissionKeys
    ) : IRequest<bool>;
}
