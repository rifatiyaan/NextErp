using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NextErp.Application.Commands.Identity;
using NextErp.Application.DTOs;
using NextErp.Application.Interfaces;
using NextErp.Application.Queries;

namespace NextErp.API.Controllers;

[Authorize(Roles = "SuperAdmin,Admin")]
[Route("api/[controller]")]
[ApiController]
public class IdentityController(
    ISender sender,
    IBranchProvider branchProvider) : ControllerBase
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

    [HttpPost("roles")]
    public async Task<IActionResult> CreateRole(
        [FromBody] CreateRoleDto dto,
        CancellationToken cancellationToken = default)
    {
        var entry = await sender.Send(
            new CreateRoleCommand(dto.Name, dto.Description),
            cancellationToken);

        return StatusCode(StatusCodes.Status201Created, entry);
    }

    [HttpPut("roles/{roleId:guid}")]
    public async Task<IActionResult> RenameRole(
        Guid roleId,
        [FromBody] RenameRoleDto dto,
        CancellationToken cancellationToken = default)
    {
        var success = await sender.Send(
            new RenameRoleCommand(roleId, dto.Name),
            cancellationToken);

        if (!success)
            return NotFound();

        return NoContent();
    }

    [HttpDelete("roles/{roleId:guid}")]
    public async Task<IActionResult> DeleteRole(
        Guid roleId,
        CancellationToken cancellationToken = default)
    {
        var success = await sender.Send(
            new DeleteRoleCommand(roleId),
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
        var success = await sender.Send(
            new PatchUserCommand(
                id,
                dto.BranchId,
                dto.RoleName,
                CallerIsSuperAdmin: User.IsInRole("SuperAdmin"),
                CallerIsGlobal: branchProvider.IsGlobal()),
            cancellationToken);

        return success ? NoContent() : NotFound();
    }

    [HttpPost("users")]
    public async Task<IActionResult> CreateUser(
        [FromBody] CreateUserDto dto,
        CancellationToken cancellationToken = default)
    {
        var entry = await sender.Send(
            new CreateUserCommand(
                dto.Email,
                dto.Password,
                dto.FirstName,
                dto.LastName,
                dto.BranchId,
                dto.RoleName,
                CallerIsSuperAdmin: User.IsInRole("SuperAdmin"),
                CallerIsGlobal: branchProvider.IsGlobal()),
            cancellationToken);

        return StatusCode(StatusCodes.Status201Created, entry);
    }
}
