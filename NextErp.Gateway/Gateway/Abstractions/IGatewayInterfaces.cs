using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.Transforms;

namespace NextErp.Gateway.Gateway.Abstractions
{
    public interface IGatewayConfigProvider : IProxyConfigProvider
    {
        void UpdateConfig(IReadOnlyList<RouteConfig> routes, IReadOnlyList<ClusterConfig> clusters);
    }

    public interface IGatewayRoutePolicy
    {
        string Name { get; }
        // Add specific policy logic methods if needed
    }

    public interface IGatewayRequestTransform
    {
        void Apply(Yarp.ReverseProxy.Transforms.RequestTransformContext context);
    }
}
