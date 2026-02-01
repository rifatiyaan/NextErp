using System;
using Yarp.ReverseProxy.Transforms;
using NextErp.Gateway.Gateway.Abstractions;

namespace NextErp.Gateway.Gateway.Transforms
{
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
    public class LoadBalancingPolicy : IGatewayRoutePolicy
    {
        public string Name => GatewayConstants.Policies.LoadBalancingPolicy;
    }

    public class RateLimitPolicy : IGatewayRoutePolicy
    {
        public string Name => GatewayConstants.Policies.RateLimitPolicy;
    }
}
