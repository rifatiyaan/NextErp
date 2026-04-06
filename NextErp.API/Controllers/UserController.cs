using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NextErp.Application.DTOs;
using NextErp.Application.Interfaces;
using NextErp.Domain.Entities;

namespace NextErp.API.Controllers;

[Authorize(Roles = "SuperAdmin,Admin")]
[Route("api/[controller]")]
[ApiController]
public class UserController(
    UserManager<ApplicationUser> userManager,
    IBranchProvider branchProvider) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] RegisterDto dto)
    {
        try
        {
            var branchId = ResolveTargetBranchId(dto.BranchId);
            var userName = string.IsNullOrWhiteSpace(dto.Username) ? dto.Email : dto.Username.Trim();

            var user = new ApplicationUser
            {
                UserName = userName,
                Email = dto.Email,
                BranchId = branchId
            };

            var result = await userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
            {
                var detail = string.Join("; ", result.Errors.Select(e => e.Description));
                return Problem(title: "User creation failed", detail: detail, statusCode: StatusCodes.Status400BadRequest);
            }

            return Ok(new
            {
                user.Id,
                user.Email,
                user.UserName,
                user.BranchId
            });
        }
        catch (InvalidOperationException ex)
        {
            return Problem(title: "Bad request", detail: ex.Message, statusCode: StatusCodes.Status400BadRequest);
        }
    }

    private Guid ResolveTargetBranchId(Guid requestedBranchId)
    {
        if (!branchProvider.IsGlobal())
            return branchProvider.GetRequiredBranchId();

        if (requestedBranchId == Guid.Empty)
            throw new InvalidOperationException("BranchId is required for global user creation.");

        return requestedBranchId;
    }
}
