using MediatR;
using NextErp.Application.Common.Attributes;
using NextErp.Application.Common.Interfaces;
using NextErp.Application.DTOs;

namespace NextErp.Application.Commands.Identity
{
    [RequiresPermission("Settings.UserControl.Manage")]
    public record CreateUserCommand(
            string Email,
            string Password,
            string? FirstName,
            string? LastName,
            Guid? BranchId,
            string? RoleName,
            bool CallerIsSuperAdmin,
            bool CallerIsGlobal
        ) : IRequest<IdentityUserEntry>, ITransactionalRequest;
}
