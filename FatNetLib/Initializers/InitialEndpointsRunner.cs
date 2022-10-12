using Kolyhalov.FatNetLib.Endpoints;
using Kolyhalov.FatNetLib.Microtypes;
using LiteNetLib;
using Newtonsoft.Json.Linq;

namespace Kolyhalov.FatNetLib.Initializers;

// Todo: make this class json-independent
public class InitialEndpointsRunner : IInitialEndpointsRunner
{
    private const int ServerPeerId = 0;
    private readonly Route _initialEndpointsHoldAndGetRoute = new("fat-net-lib/init-endpoints/hold-and-get");
    private readonly IClient _client;
    private readonly IEndpointsStorage _endpointsStorage;
    private readonly IDependencyContext _context;

    public InitialEndpointsRunner(IClient client,
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
        IList<Route> initialRoutes = ExtractRoutes(response);
        RegisterInitialEndpoints(initialRoutes);
        CallInitialEndpoints(initialRoutes);
    }

    private void RegisterInitialEndpointsGetter(IEndpointsStorage endpointsStorage)
    {
        Endpoint endpoint = CreateInitialEndpoint(_initialEndpointsHoldAndGetRoute);
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
            Route = _initialEndpointsHoldAndGetRoute,
            Context = _context,
            Body = new Dictionary<string, object>
            {
                {
                    "Endpoints", _endpointsStorage.LocalEndpoints.Select(x => x.EndpointData).Where(x => x.IsInitial)
                }
            }
        };
        return _client.SendPackage(request, ServerPeerId)!;
    }

    private static IList<Route> ExtractRoutes(Package package)
    {
        var routesJArray = (JArray) package.Body!["Endpoints"];
        return routesJArray.Select(routeJToken => routeJToken.ToObject<string>()!)
            .Select(routeString => new Route(routeString))
            .ToList();
    }

    private static Endpoint CreateInitialEndpoint(Route route) =>
        new(route, EndpointType.Exchanger, DeliveryMethod.ReliableOrdered, true);

    private void RegisterInitialEndpoints(IList<Route> routes)
    {
        foreach (Route route in routes)
        {
            Endpoint endpoint = CreateInitialEndpoint(route);
            _endpointsStorage.RemoteEndpoints[ServerPeerId].Add(endpoint);
        }
    }

    private void CallInitialEndpoints(IList<Route> routes)
    {
        foreach (Route route in routes)
        {
            var package = new Package
            {
                Route = route,
                Context = _context
            };
            Package _ = _client.SendPackage(package, ServerPeerId)!;
        }
    }
}