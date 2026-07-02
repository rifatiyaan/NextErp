using Microsoft.Extensions.Primitives;

namespace NextErp.Application.Common.Caching;

// Invalidation signal for cached Module reads (the per-tenant menu tree).
public interface IModuleCacheSignal
{
    IChangeToken Token { get; }
    void Invalidate();
}

public sealed class ModuleCacheSignal : CacheSignal, IModuleCacheSignal;
