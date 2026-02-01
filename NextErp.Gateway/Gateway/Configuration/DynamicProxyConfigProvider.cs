using Microsoft.Extensions.Primitives;
using Yarp.ReverseProxy.Configuration;
using System.Threading;
using System.Collections.Concurrent;
using System.Linq;
using NextErp.Gateway.Gateway.Abstractions;

namespace NextErp.Gateway.Gateway.Configuration
{
    public class ProxyConfigSnapshot : IProxyConfig
    {
        private readonly CancellationTokenSource _cts = new();

        public ProxyConfigSnapshot(IReadOnlyList<RouteConfig> routes, IReadOnlyList<ClusterConfig> clusters)
        {
            Routes = routes;
            Clusters = clusters;
            ChangeToken = new CancellationChangeToken(_cts.Token);
        }

        public IReadOnlyList<RouteConfig> Routes { get; }
        public IReadOnlyList<ClusterConfig> Clusters { get; }
        public IChangeToken ChangeToken { get; }

        internal void SignalChange() => _cts.Cancel();
    }

    public class DynamicProxyConfigProvider : IGatewayConfigProvider
    {
        private readonly ConcurrentDictionary<string, RouteConfig> _routes = new();
        private readonly ConcurrentDictionary<string, ClusterConfig> _clusters = new();
        private volatile ProxyConfigSnapshot _snapshot;

        public DynamicProxyConfigProvider(IReadOnlyList<RouteConfig> routes, IReadOnlyList<ClusterConfig> clusters)
        {
            foreach (var r in routes) _routes[r.RouteId] = r;
            foreach (var c in clusters) _clusters[c.ClusterId] = c;
            _snapshot = new ProxyConfigSnapshot(routes, clusters);
        }

        public IProxyConfig GetConfig() => _snapshot;

        public void UpdateConfig(IReadOnlyList<RouteConfig> routes, IReadOnlyList<ClusterConfig> clusters)
        {
            _routes.Clear();
            _clusters.Clear();
            foreach (var r in routes) _routes[r.RouteId] = r;
            foreach (var c in clusters) _clusters[c.ClusterId] = c;
            
            SynchronizeSnapshot();
        }

        private void SynchronizeSnapshot()
        {
            var newSnapshot = new ProxyConfigSnapshot(
                _routes.Values.ToList().AsReadOnly(),
                _clusters.Values.ToList().AsReadOnly());

            var oldSnapshot = Interlocked.Exchange(ref _snapshot, newSnapshot);
            oldSnapshot.SignalChange();
        }
    }
}
