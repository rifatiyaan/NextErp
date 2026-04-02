using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NextErp.Application.Commands.Identity;
using NextErp.Application.Interfaces;
using NextErp.Domain.Entities;

namespace NextErp.Application.Handlers.CommandHandlers.Identity
{
    /// <summary>
    /// Atomically replaces all permission keys for a role:
    /// delete old rows, insert new rows, all in one SaveChanges call.
    /// </summary>
    public class SetRolePermissionsHandler(
        IApplicationDbContext dbContext,
        RoleManager<IdentityRole<Guid>> roleManager)
        : IRequestHandler<SetRolePermissionsCommand, bool>
    {
        public async Task<bool> Handle(
            SetRolePermissionsCommand request,
            CancellationToken cancellationToken)
        {
            var roleExists = await roleManager.Roles
                .AnyAsync(r => r.Id == request.RoleId, cancellationToken);

            if (!roleExists)
                return false;

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
