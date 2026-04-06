using MediatR;
using NextErp.Application.Common.Interfaces;

namespace NextErp.Application.Commands.Identity
{
    public record SetRolePermissionsCommand(
            Guid RoleId,
            IReadOnlyList<string> PermissionKeys
        ) : IRequest<bool>, ITransactionalRequest;
}
