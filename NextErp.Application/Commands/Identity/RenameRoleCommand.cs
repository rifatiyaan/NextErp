using MediatR;
using NextErp.Application.Common.Attributes;
using NextErp.Application.Common.Interfaces;

namespace NextErp.Application.Commands.Identity
{
    [RequiresPermission("Settings.UserControl.Manage")]
    public record RenameRoleCommand(
            Guid RoleId,
            string NewName
        ) : IRequest<bool>, ITransactionalRequest;
}
