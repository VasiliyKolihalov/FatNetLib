using Kolyhalov.FatNetLib.Endpoints;

namespace Kolyhalov.FatNetLib.Middlewares;

public class PutLocalEndpointsMiddleware : IMiddleware
{
    public void Process(Package package)
    {
        if (package.Route!.ToString() != "fat-net-lib/endpoints/exchange")
            return;

        package.Body = new Dictionary<string, object>
        {
            ["Endpoints"] = package.Context!
                .Get<IEndpointsStorage>()
                .LocalEndpoints
                .Select(_ => _.EndpointData)
        };
    }
}