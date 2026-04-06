using MediatR;

namespace NextErp.Application.Commands.Identity
{
    public record PatchUserCommand(
            Guid UserId,
            Guid? BranchId,
            string? RoleName
        ) : IRequest<bool>;
}
