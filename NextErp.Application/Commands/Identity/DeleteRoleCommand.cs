using MediatR;
using NextErp.Application.Common.Attributes;
using NextErp.Application.Common.Interfaces;

namespace NextErp.Application.Commands.Identity
{
    [RequiresPermission("Settings.UserControl.Manage")]
    public record DeleteRoleCommand(
            Guid RoleId
        ) : IRequest<bool>, ITransactionalRequest;
}
