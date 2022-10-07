using Kolyhalov.FatNetLib.Endpoints;
using Newtonsoft.Json;

namespace Kolyhalov.FatNetLib.Middlewares;

public class RegisterRemoteEndpointsMiddleware : IMiddleware
{
    public void Process(Package package)
    {
        if (package.Route!.ToString() != "fat-net-lib/endpoints/exchange")
            return;

        IDependencyContext context = package.Context ?? throw new FatNetLibException(
            $"{nameof(package.Context)} field is missing");
        int fromPeerId = package.FromPeerId ?? throw new FatNetLibException(
            $"{nameof(package.FromPeerId)} field is missing");

        var endpointsJson = package.Body!["Endpoints"].ToString()!;
        JsonConverter[] jsonConverters = context.Get<JsonSerializer>().Converters.ToArray();
        var endpoints = JsonConvert.DeserializeObject<IList<Endpoint>>(endpointsJson, jsonConverters)!;

        IDictionary<int, IList<Endpoint>> remoteEndpoints = context.Get<IEndpointsStorage>().RemoteEndpoints;
        remoteEndpoints[fromPeerId] = remoteEndpoints.ContainsKey(fromPeerId)
            ? remoteEndpoints[fromPeerId].Concat(endpoints).ToList()
            : endpoints;
    }
}