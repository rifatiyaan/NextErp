using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NextErp.API.DTO;
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
                return BadRequest(result.Errors);

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
            return BadRequest(new { message = ex.Message });
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
