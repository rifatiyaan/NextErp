using MediatR;
using Microsoft.AspNetCore.Identity;
using NextErp.Application.Commands.Identity;
using NextErp.Application.Common.Exceptions;
using NextErp.Application.Interfaces;
using NextErp.Domain.Entities;

namespace NextErp.Application.Handlers.CommandHandlers.Identity
{
    public class PatchUserHandler(
        UserManager<ApplicationUser> userManager,
        IBranchProvider branchProvider)
            : IRequestHandler<PatchUserCommand, bool>
    {
        public async Task<bool> Handle(PatchUserCommand request, CancellationToken cancellationToken = default)
        {
            var user = await userManager.FindByIdAsync(request.UserId.ToString());
            if (user is null)
                return false;

            // Only SuperAdmins can modify other SuperAdmins.
            var targetRoles = await userManager.GetRolesAsync(user);
            var targetIsSuperAdmin = targetRoles.Any(r =>
                string.Equals(r, "SuperAdmin", StringComparison.OrdinalIgnoreCase));
            if (targetIsSuperAdmin && !request.CallerIsSuperAdmin)
                throw new ForbiddenAccessException("Only SuperAdmin can modify a SuperAdmin user.");

            // Only SuperAdmin can assign the SuperAdmin role.
            if (!string.IsNullOrWhiteSpace(request.RoleName)
                && string.Equals(request.RoleName, "SuperAdmin", StringComparison.OrdinalIgnoreCase)
                && !request.CallerIsSuperAdmin)
            {
                throw new ForbiddenAccessException("Only SuperAdmin can assign the SuperAdmin role.");
            }

            // Non-global callers may only patch users in their own branch and may
            // not move users to a different branch.
            if (!request.CallerIsGlobal)
            {
                var callerBranch = branchProvider.GetRequiredBranchId();
                if (user.BranchId != callerBranch)
                    throw new ForbiddenAccessException("You can only modify users in your own branch.");
                if (request.BranchId is { } b && b != Guid.Empty && b != callerBranch)
                    throw new ForbiddenAccessException("You can only assign users to your own branch.");
            }

            if (request.BranchId.HasValue && request.BranchId.Value != Guid.Empty)
            {
                user.BranchId = request.BranchId.Value;
                user.UpdatedAt = DateTime.UtcNow;

                var updateResult = await userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                    return false;
            }

            if (!string.IsNullOrWhiteSpace(request.RoleName))
            {
                var currentRoles = await userManager.GetRolesAsync(user);
                if (currentRoles.Any())
                    await userManager.RemoveFromRolesAsync(user, currentRoles);

                await userManager.AddToRoleAsync(user, request.RoleName);
            }

            return true;
        }
    }
}
