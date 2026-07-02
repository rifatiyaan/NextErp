using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NextErp.Application.Commands.Identity;
using NextErp.Application.Common.Caching;
using NextErp.Application.Interfaces;
using NextErp.Domain.Entities;

namespace NextErp.Application.Handlers.CommandHandlers.Identity
{
    public class DeleteRoleHandler(
            IApplicationDbContext dbContext,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole<Guid>> roleManager,
            IPermissionCacheSignal? cacheSignal = null)
            : IRequestHandler<DeleteRoleCommand, bool>
    {
        private static readonly HashSet<string> ProtectedRoles = new(StringComparer.OrdinalIgnoreCase)
        {
            "SuperAdmin",
            "Admin"
        };

        public async Task<bool> Handle(
            DeleteRoleCommand request,
            CancellationToken cancellationToken = default)
        {
            var role = await roleManager.FindByIdAsync(request.RoleId.ToString());
            if (role is null)
                return false;

            if (!string.IsNullOrEmpty(role.Name) && ProtectedRoles.Contains(role.Name))
                throw new InvalidOperationException(
                    $"The '{role.Name}' role is a system role and cannot be deleted.");

            var usersInRole = await userManager.GetUsersInRoleAsync(role.Name ?? string.Empty);
            if (usersInRole.Count > 0)
                throw new InvalidOperationException(
                    $"Cannot delete role with {usersInRole.Count} user(s). Reassign users first.");

            // Cascade: clear out role permissions for this role.
            var rolePermissions = await dbContext.RolePermissions
                .Where(rp => rp.RoleId == role.Id)
                .ToListAsync(cancellationToken);

            if (rolePermissions.Count > 0)
            {
                dbContext.RolePermissions.RemoveRange(rolePermissions);
                await dbContext.SaveChangesAsync(cancellationToken);
            }

            var deleteResult = await roleManager.DeleteAsync(role);
            if (!deleteResult.Succeeded)
            {
                var detail = string.Join("; ", deleteResult.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Could not delete role: {detail}");
            }

            cacheSignal?.Invalidate();
            return true;
        }
    }
}
