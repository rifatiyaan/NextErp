using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.Transforms;

namespace NextErp.Gateway.Gateway.Abstractions
{
    /// <summary>
    /// Custom abstraction for dynamic proxy configuration management.
    /// </summary>
    public interface IGatewayConfigProvider : IProxyConfigProvider
    {
        void UpdateConfig(IReadOnlyList<RouteConfig> routes, IReadOnlyList<ClusterConfig> clusters);
    }

    /// <summary>
    /// Abstraction for custom route policies.
    /// </summary>
    public interface IGatewayRoutePolicy
    {
        string Name { get; }
        // Add specific policy logic methods if needed
    }

    /// <summary>
    /// Abstraction for custom request transformations.
    /// </summary>
    public interface IGatewayRequestTransform
    {
        void Apply(Yarp.ReverseProxy.Transforms.RequestTransformContext context);
    }
}
