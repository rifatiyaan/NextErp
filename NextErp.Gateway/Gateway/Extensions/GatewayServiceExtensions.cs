using Microsoft.Extensions.DependencyInjection;
using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.Transforms;
using NextErp.Gateway.Gateway.Abstractions;
using NextErp.Gateway.Gateway.Configuration;
using NextErp.Gateway.Gateway.Transforms;
using NextErp.Gateway.Gateway.Policies;

namespace NextErp.Gateway.Gateway.Extensions
{
    public static class GatewayServiceExtensions
    {
        public static IReverseProxyBuilder AddNextErpGateway(this IServiceCollection services)
        {
            // Initial empty configuration
            var initialRoutes = new List<RouteConfig>();
            var initialClusters = new List<ClusterConfig>();

            // Register the custom config provider as a singleton
            var configProvider = new DynamicProxyConfigProvider(initialRoutes, initialClusters);
            services.AddSingleton<IGatewayConfigProvider>(configProvider);
            services.AddSingleton<IProxyConfigProvider>(configProvider);

            // Add YARP with our custom provider
            var builder = services.AddReverseProxy();

            // Register custom components
            services.AddSingleton<IGatewayRequestTransform, HeaderTransform>();
            services.AddSingleton<IGatewayRoutePolicy, LoadBalancingPolicy>();
            services.AddSingleton<IGatewayRoutePolicy, RateLimitPolicy>();

            return builder;
        }

        public static IReverseProxyBuilder AddGatewayTransforms(this IReverseProxyBuilder builder)
        {
            builder.AddTransforms(context =>
            {
                context.AddRequestTransform(async transformContext =>
                {
                    var transform = transformContext.HttpContext.RequestServices.GetRequiredService<IGatewayRequestTransform>();
                    transform.Apply(transformContext);
                    await Task.CompletedTask;
                });
            });

            return builder;
        }
    }
}
