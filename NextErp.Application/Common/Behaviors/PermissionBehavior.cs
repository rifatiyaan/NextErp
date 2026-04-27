using System.Reflection;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NextErp.Application.Common.Attributes;
using NextErp.Application.Common.Exceptions;
using NextErp.Application.Interfaces;

namespace NextErp.Application.Common.Behaviors;

public sealed class PermissionBehavior<TRequest, TResponse>(
    IUserContext userContext,
    IApplicationDbContext db,
    IServiceProvider services)
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

        foreach (var permission in required)
        {
            var allowed = await db.RolePermissions
                .AsNoTracking()
                .AnyAsync(rp => rp.RoleId == roleId && rp.PermissionKey == permission, cancellationToken)
                .ConfigureAwait(false);

            if (!allowed)
                throw new ForbiddenAccessException($"Missing permission: {permission}");
        }

        return await next(cancellationToken).ConfigureAwait(false);
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
