using System;
using Yarp.ReverseProxy.Transforms;
using NextErp.Gateway.Gateway.Abstractions;

namespace NextErp.Gateway.Gateway.Transforms
{
    /// <summary>
    /// Custom transformation to add security or tracing headers.
    /// </summary>
    public class HeaderTransform : IGatewayRequestTransform
    {
        public void Apply(Yarp.ReverseProxy.Transforms.RequestTransformContext context)
        {
            context.ProxyRequest.Headers.Add("X-Gateway-Trace-Id", Guid.NewGuid().ToString());
        }
    }
}

namespace NextErp.Gateway.Gateway.Policies
{
    /// <summary>
    /// Placeholder for custom load balancing logic.
    /// </summary>
    public class LoadBalancingPolicy : IGatewayRoutePolicy
    {
        public string Name => GatewayConstants.Policies.LoadBalancingPolicy;
    }

    /// <summary>
    /// Placeholder for custom rate limiting logic.
    /// </summary>
    public class RateLimitPolicy : IGatewayRoutePolicy
    {
        public string Name => GatewayConstants.Policies.RateLimitPolicy;
    }
}
