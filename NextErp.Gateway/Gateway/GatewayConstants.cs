namespace NextErp.Gateway.Gateway
{
    public static class GatewayConstants
    {
        public const string DefaultClusterId = "default-cluster";
        public const string DefaultRouteId = "default-route";
        public const string ConfigurationSectionName = "ReverseProxy";
        
        public static class Policies
        {
            public const string LoadBalancingPolicy = "CustomLoadBalancing";
            public const string RateLimitPolicy = "CustomRateLimit";
        }
    }
}
