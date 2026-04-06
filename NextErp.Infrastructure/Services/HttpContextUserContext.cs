using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using NextErp.Application.Interfaces;

namespace NextErp.Infrastructure.Services;

public sealed class HttpContextUserContext(IHttpContextAccessor httpContextAccessor) : IUserContext
{
    private ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated == true;

    public Guid? UserId =>
        Guid.TryParse(User?.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

    public Guid? PrimaryRoleId =>
        Guid.TryParse(User?.FindFirstValue("primaryRoleId"), out var rid) ? rid : null;

    public bool IsSuperAdmin =>
        string.Equals(User?.FindFirstValue("isSuperAdmin"), "true", StringComparison.OrdinalIgnoreCase);
}
