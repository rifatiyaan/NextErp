using System.Reflection;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using NextErp.Application.Common.Attributes;
using NextErp.Application.Common.Caching;
using NextErp.Application.Common.Exceptions;
using NextErp.Application.Interfaces;

namespace NextErp.Application.Common.Behaviors;

public sealed class PermissionBehavior<TRequest, TResponse>(
    IUserContext userContext,
    IApplicationDbContext db,
    IServiceProvider services,
    IMemoryCache cache,
    IPermissionCacheSignal cacheSignal)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken = default)
    {
        var handlerType = ResolveHandlerType();
        var required = GetRequiredPermissions(typeof(TRequest), handlerType);
        if (required.Count == 0)
            return await next(cancellationToken).ConfigureAwait(false);

        if (!userContext.IsAuthenticated)
            throw new UnauthorizedAccessException("Authentication required.");

        if (userContext.IsSuperAdmin)
            return await next(cancellationToken).ConfigureAwait(false);

        if (userContext.PrimaryRoleId is not Guid roleId)
            throw new ForbiddenAccessException("No role is assigned for permission checks.");

        var grantedSet = await GetGrantedPermissionsAsync(roleId, cancellationToken).ConfigureAwait(false);

        var missing = required.FirstOrDefault(p => !grantedSet.Contains(p));
        if (missing is not null)
            throw new ForbiddenAccessException($"Missing permission: {missing}");

        return await next(cancellationToken).ConfigureAwait(false);
    }

    // The whole granted set for a role is cached (small, reused by every
    // guarded endpoint) instead of querying per required-permission subset.
    // This runs for every authorized request — the hottest read in the app.
    private async Task<HashSet<string>> GetGrantedPermissionsAsync(Guid roleId, CancellationToken cancellationToken)
    {
        var cacheKey = $"perms:role:{roleId}";
        if (cache.TryGetValue(cacheKey, out HashSet<string>? cached) && cached is not null)
            return cached;

        var granted = await db.RolePermissions
            .AsNoTracking()
            .Where(rp => rp.RoleId == roleId)
            .Select(rp => rp.PermissionKey)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        // Keys are lowercased on write (SetRolePermissionsHandler) while
        // [RequiresPermission] values are mixed-case — compare case-insensitively,
        // matching the MSSQL default collation the old SQL-side filter relied on.
        var grantedSet = new HashSet<string>(granted, StringComparer.OrdinalIgnoreCase);

        // Short TTL as a safety net; the signal evicts synchronously on role edits.
        cache.Set(cacheKey, grantedSet, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
        }.AddExpirationToken(cacheSignal.Token));

        return grantedSet;
    }

    private Type ResolveHandlerType()
    {
        var handlerInterface = typeof(IRequestHandler<,>).MakeGenericType(typeof(TRequest), typeof(TResponse));
        var handler = services.GetService(handlerInterface);
        return handler?.GetType() ?? typeof(object);
    }

    private static List<string> GetRequiredPermissions(Type requestType, Type handlerType)
    {
        var fromRequest = requestType.GetCustomAttributes<RequiresPermissionAttribute>(inherit: true);
        var fromHandler = handlerType.GetCustomAttributes<RequiresPermissionAttribute>(inherit: true);
        return fromRequest
            .Concat(fromHandler)
            .Select(a => a.Permission)
            .Distinct(StringComparer.Ordinal)
            .ToList();
    }
}
