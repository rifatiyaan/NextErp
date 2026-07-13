using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NextErp.API.Security;
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
    IWebHostEnvironment env,
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

            // Anonymous self-registration signs the new account in via cookies,
            // exactly like login. But when an authenticated admin is CREATING a
            // user for someone else, we must NOT overwrite the admin's own auth
            // cookies — issue nothing in that case.
            if (!isAuthenticatedRequest)
            {
                var rawRefresh = await IssueRefreshTokenAsync(user.Id);
                IssueAuthCookies(token, rawRefresh);
            }

            logger.LogInformation(
                "Register succeeded for Email={Email} in {ElapsedMs}ms",
                dto.Email,
                (DateTimeOffset.UtcNow - startedAt).TotalMilliseconds);

            return Ok(new { token = string.Empty });
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

    [EnableRateLimiting("auth")]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var user = await userManager.FindByEmailAsync(dto.Email);
        if (user == null) return Unauthorized();

        // lockoutOnFailure: true → each wrong password increments
        // AccessFailedCount; the 5th within the window sets LockoutEnd and every
        // attempt after that returns IsLockedOut until it expires. A success
        // resets the counter to zero.
        var result = await signInManager.CheckPasswordSignInAsync(user, dto.Password, lockoutOnFailure: true);

        if (result.IsLockedOut)
        {
            logger.LogWarning("Login blocked: account {Email} is locked out.", dto.Email);
            return StatusCode(StatusCodes.Status423Locked,
                "Too many failed attempts. This account is temporarily locked. Try again later.");
        }

        if (!result.Succeeded)
            return Unauthorized();

        var response = await BuildLoginResponseAsync(user);

        // Issue a fresh refresh token (store only its hash) and set the auth
        // cookies. The access token now travels as an httpOnly cookie, so it is
        // deliberately NOT returned in the body — nothing sensitive touches JS.
        var rawRefresh = await IssueRefreshTokenAsync(user.Id);
        IssueAuthCookies(response.Token, rawRefresh);

        return Ok(new LoginResponseDto
        {
            Token = string.Empty,
            IsSuperAdmin = response.IsSuperAdmin,
            PermissionKeys = response.PermissionKeys,
        });
    }

    [EnableRateLimiting("auth")]
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(CancellationToken cancellationToken = default)
    {
        var raw = Request.Cookies[AuthCookieNames.Refresh];
        if (string.IsNullOrEmpty(raw)) return Unauthorized();

        var hash = Sha256Hex(raw);
        var stored = await dbContext.RefreshTokens
            .FirstOrDefaultAsync(t => t.TokenHash == hash, cancellationToken);

        // Unknown token, or an already-rotated/expired one being replayed:
        // treat replay of a dead token as theft — revoke the user's whole
        // active set and force a fresh login.
        if (stored is null)
        {
            ClearAuthCookies();
            return Unauthorized();
        }
        if (stored.RevokedAt is not null || DateTime.UtcNow >= stored.ExpiresAt)
        {
            await RevokeAllForUserAsync(stored.UserId, cancellationToken);
            ClearAuthCookies();
            return Unauthorized();
        }

        var user = await userManager.FindByIdAsync(stored.UserId.ToString());
        if (user is null)
        {
            ClearAuthCookies();
            return Unauthorized();
        }

        // Rotate: mint a new access token + new refresh token, mark the old
        // refresh token revoked and point it at its replacement.
        var (roles, primaryRoleId, isSuperAdmin) = await ResolveRoleContextAsync(user);
        var access = await GenerateJwtTokenAsync(user, roles, primaryRoleId, isSuperAdmin);

        var newRaw = GenerateRawToken();
        var newHash = Sha256Hex(newRaw);
        stored.RevokedAt = DateTime.UtcNow;
        stored.ReplacedByTokenHash = newHash;
        dbContext.RefreshTokens.Add(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = newHash,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(RefreshTokenDays),
        });
        await dbContext.SaveChangesAsync(cancellationToken);

        IssueAuthCookies(access, newRaw);
        return NoContent();
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken = default)
    {
        var raw = Request.Cookies[AuthCookieNames.Refresh];
        if (!string.IsNullOrEmpty(raw))
        {
            var hash = Sha256Hex(raw);
            var stored = await dbContext.RefreshTokens
                .FirstOrDefaultAsync(t => t.TokenHash == hash, cancellationToken);
            if (stored is not null && stored.RevokedAt is null)
            {
                stored.RevokedAt = DateTime.UtcNow;
                await dbContext.SaveChangesAsync(cancellationToken);
            }
        }

        ClearAuthCookies();
        return NoContent();
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
            expires: now.AddMinutes(AccessTokenMinutes),
            claims: userClaims,
            signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    // ===================================================================
    // 🔹 REFRESH TOKEN + COOKIE HELPERS
    // ===================================================================

    // Short access token; long refresh token. Configurable, with safe defaults.
    private int AccessTokenMinutes => configuration.GetValue<int?>("Jwt:AccessTokenMinutes") ?? 15;
    private int RefreshTokenDays => configuration.GetValue<int?>("Jwt:RefreshTokenDays") ?? 7;

    // Secure cookies require https. Dev runs plain http://localhost, so Secure is
    // off there; everywhere else it's on. SameSite: Lax is enough same-site (dev),
    // None (with Secure) is needed if the API and SPA are on different sites in prod.
    private bool CookieSecure => !env.IsDevelopment();
    private SameSiteMode CookieSameSite => env.IsDevelopment() ? SameSiteMode.Lax : SameSiteMode.None;

    // 32 random bytes as lowercase hex — cookie-safe (no +, /, =) and unguessable.
    private static string GenerateRawToken() =>
        Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLowerInvariant();

    private static string Sha256Hex(string input) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(input))).ToLowerInvariant();

    private async Task<string> IssueRefreshTokenAsync(Guid userId)
    {
        var raw = GenerateRawToken();
        dbContext.RefreshTokens.Add(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = Sha256Hex(raw),
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(RefreshTokenDays),
        });
        await dbContext.SaveChangesAsync();
        return raw;
    }

    private async Task RevokeAllForUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        var active = await dbContext.RefreshTokens
            .Where(t => t.UserId == userId && t.RevokedAt == null)
            .ToListAsync(cancellationToken);
        foreach (var t in active)
            t.RevokedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private void IssueAuthCookies(string accessToken, string rawRefreshToken)
    {
        Response.Cookies.Append(AuthCookieNames.Access, accessToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = CookieSecure,
            SameSite = CookieSameSite,
            Path = "/",
            Expires = DateTimeOffset.UtcNow.AddMinutes(AccessTokenMinutes),
        });

        Response.Cookies.Append(AuthCookieNames.Refresh, rawRefreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = CookieSecure,
            SameSite = CookieSameSite,
            Path = AuthCookieNames.RefreshPath,
            Expires = DateTimeOffset.UtcNow.AddDays(RefreshTokenDays),
        });

        // Readable by JS (not httpOnly) so the SPA can copy it into X-CSRF-Token.
        Response.Cookies.Append(AuthCookieNames.Csrf, GenerateRawToken(), new CookieOptions
        {
            HttpOnly = false,
            Secure = CookieSecure,
            SameSite = CookieSameSite,
            Path = "/",
            Expires = DateTimeOffset.UtcNow.AddDays(RefreshTokenDays),
        });
    }

    private void ClearAuthCookies()
    {
        // Delete must match the Path the cookie was set with, or the browser
        // keeps the original.
        Response.Cookies.Delete(AuthCookieNames.Access,
            new CookieOptions { Path = "/", Secure = CookieSecure, SameSite = CookieSameSite });
        Response.Cookies.Delete(AuthCookieNames.Refresh,
            new CookieOptions { Path = AuthCookieNames.RefreshPath, Secure = CookieSecure, SameSite = CookieSameSite });
        Response.Cookies.Delete(AuthCookieNames.Csrf,
            new CookieOptions { Path = "/", Secure = CookieSecure, SameSite = CookieSameSite });
    }
}
