using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NextErp.Application.Commands.Identity;
using NextErp.Application.Interfaces;
using NextErp.Domain.Entities;

namespace NextErp.Application.Handlers.CommandHandlers.Identity
{
    public class SetRolePermissionsHandler(
            IApplicationDbContext dbContext,
            RoleManager<IdentityRole<Guid>> roleManager)
            : IRequestHandler<SetRolePermissionsCommand, bool>
    {
        public async Task<bool> Handle(
            SetRolePermissionsCommand request,
            CancellationToken cancellationToken = default)
        {
            var role = await roleManager.Roles
                .FirstOrDefaultAsync(r => r.Id == request.RoleId, cancellationToken);

            if (role is null)
                return false;

            // SuperAdmin bypasses all permission checks at runtime, so editing its
            // permission rows is misleading and could imply restrictions that do
            // not actually apply. Block writes here as a safety net.
            if (!string.IsNullOrEmpty(role.Name)
                && string.Equals(role.Name, "SuperAdmin", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "Cannot modify SuperAdmin permissions; the role bypasses all checks by design.");
            }

            // Remove all existing permissions for this role
            var existing = await dbContext.RolePermissions
                .Where(rp => rp.RoleId == request.RoleId)
                .ToListAsync(cancellationToken);

            dbContext.RolePermissions.RemoveRange(existing);

            // Add the new set
            var now = DateTime.UtcNow;
            var newRows = request.PermissionKeys
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Select(key => new RolePermission
                {
                    Id = Guid.NewGuid(),
                    RoleId = request.RoleId,
                    PermissionKey = key.ToLowerInvariant(),
                    CreatedAt = now
                });

            await dbContext.RolePermissions.AddRangeAsync(newRows, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
