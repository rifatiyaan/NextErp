using MediatR;
using NextErp.Application.Common.Interfaces;

namespace NextErp.Application.Commands.Identity
{
    public record PatchUserCommand(
            Guid UserId,
            Guid? BranchId,
            string? RoleName
        ) : IRequest<bool>, ITransactionalRequest;
}
