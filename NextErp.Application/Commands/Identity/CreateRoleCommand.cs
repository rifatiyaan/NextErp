using MediatR;
using NextErp.Application.Common.Attributes;
using NextErp.Application.Common.Interfaces;
using NextErp.Application.DTOs;

namespace NextErp.Application.Commands.Identity
{
    [RequiresPermission("Settings.UserControl.Manage")]
    public record CreateRoleCommand(
            string Name,
            string? Description = null
        ) : IRequest<IdentityRoleEntry>, ITransactionalRequest;
}
