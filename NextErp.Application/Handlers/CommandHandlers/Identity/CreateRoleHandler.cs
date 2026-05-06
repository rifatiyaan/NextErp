using MediatR;
using Microsoft.AspNetCore.Identity;
using NextErp.Application.Commands.Identity;
using NextErp.Application.DTOs;

namespace NextErp.Application.Handlers.CommandHandlers.Identity
{
    public class CreateRoleHandler(RoleManager<IdentityRole<Guid>> roleManager)
            : IRequestHandler<CreateRoleCommand, IdentityRoleEntry>
    {
        public async Task<IdentityRoleEntry> Handle(
            CreateRoleCommand request,
            CancellationToken cancellationToken = default)
        {
            var name = request.Name.Trim();

            if (await roleManager.RoleExistsAsync(name))
                throw new InvalidOperationException($"Role '{name}' already exists.");

            var role = new IdentityRole<Guid>
            {
                Id = Guid.NewGuid(),
                Name = name,
                NormalizedName = name.ToUpperInvariant()
            };

            var result = await roleManager.CreateAsync(role);
            if (!result.Succeeded)
            {
                var detail = string.Join("; ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Could not create role: {detail}");
            }

            return new IdentityRoleEntry
            {
                Id = role.Id.ToString(),
                Name = role.Name ?? name,
                UserCount = 0,
                PermissionSummary = "No permissions",
                Permissions = new List<string>()
            };
        }
    }
}
