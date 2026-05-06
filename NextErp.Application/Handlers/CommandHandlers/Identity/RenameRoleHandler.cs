using MediatR;
using Microsoft.AspNetCore.Identity;
using NextErp.Application.Commands.Identity;

namespace NextErp.Application.Handlers.CommandHandlers.Identity
{
    public class RenameRoleHandler(RoleManager<IdentityRole<Guid>> roleManager)
            : IRequestHandler<RenameRoleCommand, bool>
    {
        public async Task<bool> Handle(
            RenameRoleCommand request,
            CancellationToken cancellationToken = default)
        {
            var role = await roleManager.FindByIdAsync(request.RoleId.ToString());
            if (role is null)
                return false;

            var newName = request.NewName.Trim();

            // SuperAdmin role cannot be renamed (system role).
            if (string.Equals(role.Name, "SuperAdmin", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("The SuperAdmin role cannot be renamed.");

            if (string.Equals(newName, "SuperAdmin", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Cannot rename a role to 'SuperAdmin'.");

            // Reject if target name already taken (case-insensitive).
            if (!string.Equals(role.Name, newName, StringComparison.OrdinalIgnoreCase))
            {
                if (await roleManager.RoleExistsAsync(newName))
                    throw new InvalidOperationException($"Role '{newName}' already exists.");
            }

            var setNameResult = await roleManager.SetRoleNameAsync(role, newName);
            if (!setNameResult.Succeeded)
            {
                var detail = string.Join("; ", setNameResult.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Could not rename role: {detail}");
            }

            var updateResult = await roleManager.UpdateAsync(role);
            if (!updateResult.Succeeded)
            {
                var detail = string.Join("; ", updateResult.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Could not rename role: {detail}");
            }

            return true;
        }
    }
}
