using MediatR;
using Microsoft.AspNetCore.Identity;
using NextErp.Application.Commands.Identity;
using NextErp.Domain.Entities;

namespace NextErp.Application.Handlers.CommandHandlers.Identity
{
    public class PatchUserHandler(UserManager<ApplicationUser> userManager)
            : IRequestHandler<PatchUserCommand, bool>
    {
        public async Task<bool> Handle(PatchUserCommand request, CancellationToken cancellationToken = default)
        {
            var user = await userManager.FindByIdAsync(request.UserId.ToString());
            if (user is null)
                return false;

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
