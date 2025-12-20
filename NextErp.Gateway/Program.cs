using NextErp.Gateway.Gateway.Extensions;
using NextErp.Gateway.Gateway.Abstractions;
using Yarp.ReverseProxy.Configuration;

var builder = WebApplication.CreateBuilder(args);

// 1. Register Gateway Module (Minimal Change)
builder.Services.AddNextErpGateway()
                .AddGatewayTransforms();

var app = builder.Build();

// 2. Populate some sample routes programmatically (Example of runtime config)
var configProvider = app.Services.GetRequiredService<IGatewayConfigProvider>();
configProvider.UpdateConfig(
    routes: new[]
    {
        new RouteConfig
        {
            RouteId = "api-route",
            ClusterId = "api-cluster",
            Match = new RouteMatch { Path = "/api/{**remainder}" }
        }
    },
    clusters: new[]
    {
        new ClusterConfig
        {
            ClusterId = "api-cluster",
            Destinations = new Dictionary<string, DestinationConfig>
            {
                { "destination1", new DestinationConfig { Address = "https://localhost:7245" } }
            }
        }
    }
);

// 3. Map YARP (Minimal Change)
app.MapReverseProxy();

app.Run();
