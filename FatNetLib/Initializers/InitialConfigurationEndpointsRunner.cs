using Kolyhalov.FatNetLib.Endpoints;
using Kolyhalov.FatNetLib.Microtypes;
using LiteNetLib;
using Newtonsoft.Json.Linq;

namespace Kolyhalov.FatNetLib.Initializers;

// Todo: make this class json-independent
public class InitialConfigurationEndpointsRunner : IInitialConfigurationEndpointsRunner
{
    private const int ServerPeerId = 0;
    private readonly Route _initialEndpointsGetterRoute = new("fat-net-lib/init-config-endpoints/get");
    private readonly IClient _client;
    private readonly IEndpointsStorage _endpointsStorage;
    private readonly IDependencyContext _context;

    public InitialConfigurationEndpointsRunner(IClient client,
        IEndpointsStorage endpointsStorage,
        IDependencyContext context)
    {
        _client = client;
        _endpointsStorage = endpointsStorage;
        _context = context;
    }

    public void Run()
    {
        RegisterInitialEndpointsGetter(_endpointsStorage);
        Package response = CallInitialEndpointsGetter();
        IList<Route> configurationRoutes = ExtractRoutes(response);
        RegisterConfigurationEndpoints(configurationRoutes);
        CallConfigurationEndpoints(configurationRoutes);
    }

    private void RegisterInitialEndpointsGetter(IEndpointsStorage endpointsStorage)
    {
        Endpoint endpoint = CreateConfigurationEndpoint(_initialEndpointsGetterRoute);
        IDictionary<int, IList<Endpoint>> remoteEndpoints = endpointsStorage.RemoteEndpoints;
        if (remoteEndpoints.ContainsKey(ServerPeerId))
        {
            remoteEndpoints[ServerPeerId].Add(endpoint);
        }
        else
        {
            remoteEndpoints[ServerPeerId] = new List<Endpoint> { endpoint };
        }
    }

    private Package CallInitialEndpointsGetter()
    {
        var request = new Package
        {
            Route = _initialEndpointsGetterRoute,
            Context = _context
        };
        Package response = _client.SendPackage(request, ServerPeerId)!;
        return response;
    }

    private static IList<Route> ExtractRoutes(Package package)
    {
        var routesJArray = (JArray)package.Body!["Endpoints"];
        return routesJArray.Select(routeJToken => routeJToken.ToObject<string>()!)
            .Select(routeString => new Route(routeString))
            .ToList();
    }

    private static Endpoint CreateConfigurationEndpoint(Route route) =>
        new(route, EndpointType.Exchanger, DeliveryMethod.ReliableOrdered);

    private void RegisterConfigurationEndpoints(IList<Route> configurationRoutes)
    {
        foreach (Route configurationRoute in configurationRoutes)
        {
            Endpoint configurationEndpoint = CreateConfigurationEndpoint(configurationRoute);
            _endpointsStorage.RemoteEndpoints[ServerPeerId].Add(configurationEndpoint);
        }
    }

    private void CallConfigurationEndpoints(IList<Route> configurationRoutes)
    {
        foreach (Route configurationRoute in configurationRoutes)
        {
            var package = new Package
            {
                Route = configurationRoute,
                Context = _context
            };
            _client.SendPackage(package, ServerPeerId);
        }
    }
}