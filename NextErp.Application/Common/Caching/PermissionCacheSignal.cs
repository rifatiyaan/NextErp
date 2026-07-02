using Microsoft.Extensions.Primitives;

namespace NextErp.Application.Common.Caching;

// Invalidation signal for cached role-permission sets (PermissionBehavior).
public interface IPermissionCacheSignal
{
    IChangeToken Token { get; }
    void Invalidate();
}

public sealed class PermissionCacheSignal : CacheSignal, IPermissionCacheSignal;
