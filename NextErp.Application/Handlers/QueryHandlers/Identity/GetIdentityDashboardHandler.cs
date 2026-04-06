using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NextErp.Application.DTOs;
using NextErp.Application.Interfaces;
using NextErp.Application.Queries;
using NextErp.Domain.Entities;

namespace NextErp.Application.Handlers.QueryHandlers.Identity
{
    public class GetIdentityDashboardHandler(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole<Guid>> roleManager,
            IApplicationDbContext dbContext,
            IBranchProvider branchProvider)
            : IRequestHandler<GetIdentityDashboardQuery, IdentityCommandCenterDto>
    {
        private static string ResolveSummary(int permCount) => permCount switch
        {
            >= 20 => "Full Access",
            >= 14 => "Full Access (Branch)",
            >= 8 => "Operational",
            >= 3 => "Limited",
            _ => "Read Only"
        };

        public async Task<IdentityCommandCenterDto> Handle(
            GetIdentityDashboardQuery request,
            CancellationToken cancellationToken)
        {
            // Sequential queries: UserManager, RoleManager, and DbContext share one EF instance;
            // concurrent ToListAsync on the same context is not supported.
            var branches = await dbContext.Branches
                .AsNoTracking()
                .Where(b => b.IsActive)
                .OrderBy(b => b.Title)
                .Select(b => new IdentityBranchEntry
                {
                    Id = b.Id,
                    Name = b.Title,
                    IsActive = b.IsActive
                })
                .ToListAsync(cancellationToken);

            var allRoles = await roleManager.Roles
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var allUsers = await userManager.Users
                .AsNoTracking()
                .Include(u => u.Branch)
                .ToListAsync(cancellationToken);

            var dbPermissions = await dbContext.RolePermissions
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            if (!branchProvider.IsGlobal())
            {
                var branchId = branchProvider.GetRequiredBranchId();
                branches = branches.Where(b => b.Id == branchId).ToList();
                allUsers = allUsers.Where(u => u.BranchId == branchId).ToList();
            }

            // Group permission keys by RoleId for O(1) lookup
            var permsByRole = dbPermissions
                .GroupBy(rp => rp.RoleId)
                .ToDictionary(g => g.Key, g => g.Select(rp => rp.PermissionKey).ToList());

            // Build user → role map (one role per user, first role wins)
            var userRoleMap = new Dictionary<Guid, (string RoleId, string RoleName)>();
            foreach (var user in allUsers)
            {
                var roles = await userManager.GetRolesAsync(user);
                var primary = roles.FirstOrDefault() ?? string.Empty;
                var roleEntity = allRoles.FirstOrDefault(r =>
                    string.Equals(r.Name, primary, StringComparison.OrdinalIgnoreCase));
                userRoleMap[user.Id] = (roleEntity?.Id.ToString() ?? string.Empty, primary);
            }

            // Build role entries — permissions come from the DB
            var roleEntries = allRoles.Select(role =>
            {
                var count = userRoleMap.Values
                    .Count(v => string.Equals(v.RoleName, role.Name, StringComparison.OrdinalIgnoreCase));

                var perms = permsByRole.TryGetValue(role.Id, out var p)
                    ? p
                    : new List<string>();

                return new IdentityRoleEntry
                {
                    Id = role.Id.ToString(),
                    Name = role.Name ?? string.Empty,
                    UserCount = count,
                    PermissionSummary = ResolveSummary(perms.Count),
                    Permissions = perms
                };
            }).ToList();

            // Build user entries
            var branchMap = branches.ToDictionary(b => b.Id, b => b.Name);
            var userEntries = allUsers.Select(user =>
            {
                userRoleMap.TryGetValue(user.Id, out var roleInfo);
                var branchName = branchMap.TryGetValue(user.BranchId, out var bn) ? bn : string.Empty;

                return new IdentityUserEntry
                {
                    Id = user.Id,
                    UserName = user.UserName ?? string.Empty,
                    Email = user.Email ?? string.Empty,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    AvatarUrl = string.IsNullOrWhiteSpace(user.Photo) ? null : user.Photo,
                    BranchId = user.BranchId,
                    BranchName = branchName,
                    RoleId = roleInfo.RoleId,
                    RoleName = roleInfo.RoleName,
                    IsEmailConfirmed = user.EmailConfirmed
                };
            }).ToList();

            return new IdentityCommandCenterDto
            {
                Roles = roleEntries,
                Users = userEntries,
                Branches = branches
            };
        }
    }
}
