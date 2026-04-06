using Microsoft.Extensions.Primitives;
using Yarp.ReverseProxy.Configuration;
using System.Threading;
using System.Collections.Concurrent;
using System.Linq;
using NextErp.Gateway.Gateway.Abstractions;

namespace NextErp.Gateway.Gateway.Configuration
{
    public class ProxyConfigSnapshot(IReadOnlyList<RouteConfig> routes, IReadOnlyList<ClusterConfig> clusters)
        : IProxyConfig
    {
        private static (CancellationTokenSource Cts, IChangeToken Token) CreateChangeInfrastructure()
        {
            var cts = new CancellationTokenSource();
            return (cts, new CancellationChangeToken(cts.Token));
        }

        private readonly (CancellationTokenSource Cts, IChangeToken Token) _change = CreateChangeInfrastructure();

        public IReadOnlyList<RouteConfig> Routes { get; } = routes;
        public IReadOnlyList<ClusterConfig> Clusters { get; } = clusters;
        public IChangeToken ChangeToken => _change.Token;

        internal void SignalChange() => _change.Cts.Cancel();
    }

    public class DynamicProxyConfigProvider(IReadOnlyList<RouteConfig> routes, IReadOnlyList<ClusterConfig> clusters)
        : IGatewayConfigProvider
    {
        private static ConcurrentDictionary<string, RouteConfig> ToRouteMap(IReadOnlyList<RouteConfig> list)
        {
            var d = new ConcurrentDictionary<string, RouteConfig>();
            foreach (var r in list) d[r.RouteId] = r;
            return d;
        }

        private static ConcurrentDictionary<string, ClusterConfig> ToClusterMap(IReadOnlyList<ClusterConfig> list)
        {
            var d = new ConcurrentDictionary<string, ClusterConfig>();
            foreach (var c in list) d[c.ClusterId] = c;
            return d;
        }

        private readonly ConcurrentDictionary<string, RouteConfig> _routes = ToRouteMap(routes);
        private readonly ConcurrentDictionary<string, ClusterConfig> _clusters = ToClusterMap(clusters);
        private volatile ProxyConfigSnapshot _snapshot = new(routes, clusters);

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
