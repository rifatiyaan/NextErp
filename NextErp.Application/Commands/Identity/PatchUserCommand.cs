using MediatR;

namespace NextErp.Application.Commands.Identity
{
    /// <summary>
    /// Quick-swap: change a user's branch, role, or both in one request.
    /// </summary>
    public record PatchUserCommand(
        Guid UserId,
        Guid? BranchId,
        string? RoleName
    ) : IRequest<bool>;
}
