using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NextErp.Application.Commands.Identity;
using NextErp.Application.DTOs;
using NextErp.Application.Interfaces;
using NextErp.Application.Queries;
using NextErp.Domain.Entities;

namespace NextErp.API.Controllers;

public class SetPermissionsDto
{
    public List<string> PermissionKeys { get; set; } = new();
}

[Authorize(Roles = "SuperAdmin,Admin")]
[Route("api/[controller]")]
[ApiController]
public class IdentityController(
    ISender sender,
    IBranchProvider branchProvider,
    UserManager<ApplicationUser> userManager) : ControllerBase
{
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard(CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(new GetIdentityDashboardQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpPut("roles/{roleId:guid}/permissions")]
    public async Task<IActionResult> SetRolePermissions(
        Guid roleId,
        [FromBody] SetPermissionsDto dto,
        CancellationToken cancellationToken = default)
    {
        var success = await sender.Send(
            new SetRolePermissionsCommand(roleId, dto.PermissionKeys),
            cancellationToken);

        if (!success)
            return NotFound();

        return NoContent();
    }

    [HttpPatch("users/{id:guid}")]
    public async Task<IActionResult> PatchUser(
        Guid id,
        [FromBody] PatchUserDto dto,
        CancellationToken cancellationToken = default)
    {
        var target = await userManager.FindByIdAsync(id.ToString());
        if (target == null)
            return NotFound();

        var targetRoles = await userManager.GetRolesAsync(target);
        var targetIsSuperAdmin = targetRoles.Any(r =>
            string.Equals(r, "SuperAdmin", StringComparison.OrdinalIgnoreCase));
        if (targetIsSuperAdmin && !User.IsInRole("SuperAdmin"))
            return Forbid();

        if (!string.IsNullOrWhiteSpace(dto.RoleName)
            && string.Equals(dto.RoleName, "SuperAdmin", StringComparison.OrdinalIgnoreCase)
            && !User.IsInRole("SuperAdmin"))
            return Forbid();

        if (!branchProvider.IsGlobal())
        {
            var callerBranch = branchProvider.GetRequiredBranchId();
            if (target.BranchId != callerBranch)
                return Forbid();
            if (dto.BranchId is { } b && b != Guid.Empty && b != callerBranch)
                return Forbid();
        }

        var success = await sender.Send(
            new PatchUserCommand(id, dto.BranchId, dto.RoleName),
            cancellationToken);

        if (!success)
            return NotFound();

        return NoContent();
    }
}
