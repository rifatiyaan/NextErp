using Microsoft.Extensions.Primitives;

namespace NextErp.Application.Common.Caching;

// Shared change-token implementation for cache invalidation signals. Cached
// entries attach Token as an expiration token; a write path calls Invalidate()
// to evict every attached entry at once, so reads and writes never need to
// agree on individual cache keys.
public abstract class CacheSignal
{
    private volatile CancellationTokenSource _cts = new();

    public IChangeToken Token => new CancellationChangeToken(_cts.Token);

    public void Invalidate()
    {
        var previous = Interlocked.Exchange(ref _cts, new CancellationTokenSource());
        previous.Cancel();
        previous.Dispose();
    }
}
