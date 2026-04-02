using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using NextErp.API.DTO;
using NextErp.Application.Interfaces;
using NextErp.Domain.Entities;

namespace NextErp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
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
            return BadRequest("BranchId is required.");
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

                return BadRequest(result.Errors);
            }

            var token = await GenerateJwtToken(user);

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

        var token = await GenerateJwtToken(user);

        return Ok(new { token });
    }

    private async Task<string> GenerateJwtToken(ApplicationUser user)
    {
        var roles = await userManager.GetRolesAsync(user);
        var isGlobal = roles.Any(r => string.Equals(r, "SuperAdmin", StringComparison.OrdinalIgnoreCase));

        var userClaims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim("branchId", user.BranchId.ToString())
        };

        if (isGlobal)
            userClaims.Add(new Claim("isGlobal", "true"));

        // roles (optional)
        foreach (var role in roles)
        {
            userClaims.Add(new Claim(ClaimTypes.Role, role));
        }

        var authSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!));

        var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"],
            audience: configuration["Jwt:Audience"],
            expires: DateTime.Now.AddHours(3),
            claims: userClaims,
            signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
