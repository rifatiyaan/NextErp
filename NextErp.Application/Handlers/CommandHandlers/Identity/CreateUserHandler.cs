using MediatR;
using Microsoft.AspNetCore.Identity;
using NextErp.Application.Commands.Identity;
using NextErp.Application.DTOs;
using NextErp.Application.Interfaces;
using NextErp.Domain.Entities;

namespace NextErp.Application.Handlers.CommandHandlers.Identity
{
    public class CreateUserHandler(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole<Guid>> roleManager,
        IBranchProvider branchProvider)
            : IRequestHandler<CreateUserCommand, IdentityUserEntry>
    {
        public async Task<IdentityUserEntry> Handle(
            CreateUserCommand request,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(request.Email))
                throw new InvalidOperationException("Email is required.");
            if (string.IsNullOrWhiteSpace(request.Password))
                throw new InvalidOperationException("Password is required.");

            // Branch resolution: non-global callers can only create users in their own branch.
            Guid branchId;
            if (!request.CallerIsGlobal)
            {
                branchId = branchProvider.GetRequiredBranchId();
            }
            else
            {
                if (request.BranchId is null || request.BranchId == Guid.Empty)
                    throw new InvalidOperationException("BranchId is required for global user creation.");
                branchId = request.BranchId.Value;
            }

            // Only SuperAdmin can mint another SuperAdmin.
            if (!string.IsNullOrWhiteSpace(request.RoleName)
                && string.Equals(request.RoleName, "SuperAdmin", StringComparison.OrdinalIgnoreCase)
                && !request.CallerIsSuperAdmin)
            {
                throw new InvalidOperationException("Only SuperAdmin can assign the SuperAdmin role.");
            }

            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = request.Email.Trim(),
                Email = request.Email.Trim(),
                FirstName = request.FirstName ?? string.Empty,
                LastName = request.LastName ?? string.Empty,
                BranchId = branchId,
                CreatedAt = DateTime.UtcNow
            };

            var createResult = await userManager.CreateAsync(user, request.Password);
            if (!createResult.Succeeded)
            {
                var detail = string.Join("; ", createResult.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"User creation failed: {detail}");
            }

            Guid? roleId = null;
            var resolvedRoleName = string.Empty;
            if (!string.IsNullOrWhiteSpace(request.RoleName))
            {
                var role = await roleManager.FindByNameAsync(request.RoleName.Trim());
                if (role is null)
                {
                    // Best-effort cleanup so we don't leave an orphan user when role is invalid.
                    await userManager.DeleteAsync(user);
                    throw new InvalidOperationException($"Role '{request.RoleName}' does not exist.");
                }

                var addToRoleResult = await userManager.AddToRoleAsync(user, role.Name ?? request.RoleName.Trim());
                if (!addToRoleResult.Succeeded)
                {
                    var detail = string.Join("; ", addToRoleResult.Errors.Select(e => e.Description));
                    await userManager.DeleteAsync(user);
                    throw new InvalidOperationException($"User creation failed: {detail}");
                }

                roleId = role.Id;
                resolvedRoleName = role.Name ?? string.Empty;
            }

            return new IdentityUserEntry
            {
                Id = user.Id,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName,
                LastName = user.LastName,
                AvatarUrl = null,
                BranchId = user.BranchId,
                BranchName = string.Empty,
                RoleId = roleId?.ToString() ?? string.Empty,
                RoleName = resolvedRoleName,
                IsEmailConfirmed = user.EmailConfirmed
            };
        }
    }
}
