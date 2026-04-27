using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NextErp.Application.Common.Security;
using NextErp.Application.DTOs;
using NextErp.Application.Interfaces;
using NextErp.Domain.Entities;

namespace NextErp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    RoleManager<IdentityRole<Guid>> roleManager,
    IApplicationDbContext dbContext,
    IBranchProvider branchProvider,
    IConfiguration configuration,
    ILogger<AuthController> logger) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var startedAt = DateTimeOffset.UtcNow;
        var isAuthenticatedRequest = User?.Identity?.IsAuthenticated == true;
        if (isAuthenticatedRequest && !branchProvider.IsGlobal())
        {
            dto.BranchId = branchProvider.GetRequiredBranchId();
        }
        else if (dto.BranchId == Guid.Empty)
        {
            return Problem(
                title: "Bad request",
                detail: "BranchId is required.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        // Allow either {username,email,password} OR {email,password}
        var userName = string.IsNullOrWhiteSpace(dto.Username) ? dto.Email : dto.Username.Trim();

        var user = new ApplicationUser
        {
            UserName = userName,
            Email = dto.Email,
            BranchId = dto.BranchId
        };

        try
        {
            logger.LogInformation("Register attempt for Email={Email}, UserName={UserName}", dto.Email, userName);

            var result = await userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
            {
                logger.LogWarning(
                    "Register failed for Email={Email}. Errors: {Errors}",
                    dto.Email,
                    string.Join("; ", result.Errors.Select(e => $"{e.Code}:{e.Description}")));

                var detail = string.Join("; ", result.Errors.Select(e => e.Description));
                return Problem(
                    title: "Registration failed",
                    detail: detail,
                    statusCode: StatusCodes.Status400BadRequest);
            }

            var (roles, primaryRoleId, isSuperAdmin) = await ResolveRoleContextAsync(user);
            var token = await GenerateJwtTokenAsync(user, roles, primaryRoleId, isSuperAdmin);

            logger.LogInformation(
                "Register succeeded for Email={Email} in {ElapsedMs}ms",
                dto.Email,
                (DateTimeOffset.UtcNow - startedAt).TotalMilliseconds);

            return Ok(new { token });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Register errored for Email={Email} after {ElapsedMs}ms", dto.Email, (DateTimeOffset.UtcNow - startedAt).TotalMilliseconds);
            return Problem(
                title: "Registration failed",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var user = await userManager.FindByEmailAsync(dto.Email);
        if (user == null) return Unauthorized();

        var result = await signInManager.CheckPasswordSignInAsync(user, dto.Password, false);

        if (!result.Succeeded)
            return Unauthorized();

        var response = await BuildLoginResponseAsync(user);
        return Ok(response);
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user == null) return Unauthorized();

        var (roles, primaryRoleId, isSuperAdmin) = await ResolveRoleContextAsync(user);

        var roleIds = new List<Guid>();
        foreach (var roleName in roles)
        {
            var role = await roleManager.FindByNameAsync(roleName);
            if (role != null) roleIds.Add(role.Id);
        }

        IReadOnlyList<string> permissions;
        if (isSuperAdmin)
        {
            permissions = await dbContext.RolePermissions
                .AsNoTracking()
                .Select(rp => rp.PermissionKey)
                .Distinct()
                .ToListAsync();
        }
        else
        {
            permissions = await dbContext.RolePermissions
                .AsNoTracking()
                .Where(rp => roleIds.Contains(rp.RoleId))
                .Select(rp => rp.PermissionKey)
                .Distinct()
                .ToListAsync();
        }

        string? branchName = null;
        if (user.BranchId != Guid.Empty)
        {
            branchName = await dbContext.Branches
                .AsNoTracking()
                .Where(b => b.Id == user.BranchId)
                .Select(b => b.Title)
                .FirstOrDefaultAsync();
        }

        var isGlobal = isSuperAdmin;

        return Ok(new CurrentUserDto
        {
            Id = user.Id,
            Email = user.Email,
            UserName = user.UserName,
            FirstName = user.FirstName,
            LastName = user.LastName,
            BranchId = user.BranchId == Guid.Empty ? null : user.BranchId,
            BranchName = branchName,
            IsSuperAdmin = isSuperAdmin,
            IsGlobal = isGlobal,
            Roles = roles.ToList(),
            Permissions = permissions,
        });
    }

    private async Task<(IList<string> Roles, Guid? PrimaryRoleId, bool IsSuperAdmin)> ResolveRoleContextAsync(
        ApplicationUser user)
    {
        var roles = await userManager.GetRolesAsync(user);
        var primaryRoleName = roles.FirstOrDefault();
        var roleEntity = !string.IsNullOrEmpty(primaryRoleName)
            ? await roleManager.FindByNameAsync(primaryRoleName)
            : null;
        var primaryRoleId = roleEntity?.Id;
        var isSuperAdmin = SuperAdminRules.IsSuperAdmin(primaryRoleName, primaryRoleId);
        return (roles, primaryRoleId, isSuperAdmin);
    }

    private async Task<LoginResponseDto> BuildLoginResponseAsync(ApplicationUser user)
    {
        var (roles, primaryRoleId, isSuperAdmin) = await ResolveRoleContextAsync(user);

        IReadOnlyList<string> permissionKeys;
        if (isSuperAdmin)
        {
            permissionKeys = await dbContext.RolePermissions
                .AsNoTracking()
                .Select(rp => rp.PermissionKey)
                .Distinct()
                .ToListAsync();
        }
        else if (primaryRoleId.HasValue)
        {
            permissionKeys = await dbContext.RolePermissions
                .AsNoTracking()
                .Where(rp => rp.RoleId == primaryRoleId.Value)
                .Select(rp => rp.PermissionKey)
                .ToListAsync();
        }
        else
        {
            permissionKeys = Array.Empty<string>();
        }

        var token = await GenerateJwtTokenAsync(user, roles, primaryRoleId, isSuperAdmin);

        return new LoginResponseDto
        {
            Token = token,
            IsSuperAdmin = isSuperAdmin,
            PermissionKeys = permissionKeys
        };
    }

    private async Task<string> GenerateJwtTokenAsync(
        ApplicationUser user,
        IList<string> roles,
        Guid? primaryRoleId,
        bool isSuperAdmin)
    {
        var isGlobal = isSuperAdmin;

        var userClaims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
            new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
            new Claim("branchId", user.BranchId.ToString())
        };

        if (primaryRoleId.HasValue)
            userClaims.Add(new Claim("primaryRoleId", primaryRoleId.Value.ToString()));

        userClaims.Add(new Claim("isSuperAdmin", isSuperAdmin ? "true" : "false"));

        if (isGlobal)
            userClaims.Add(new Claim("isGlobal", "true"));

        foreach (var role in roles)
            userClaims.Add(new Claim(ClaimTypes.Role, role));

        var authSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!));

        var now = DateTime.UtcNow;
        var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"],
            audience: configuration["Jwt:Audience"],
            notBefore: now,
            expires: now.AddHours(3),
            claims: userClaims,
            signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
