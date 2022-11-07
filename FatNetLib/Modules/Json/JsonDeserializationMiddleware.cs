using Kolyhalov.FatNetLib.Endpoints;
using Kolyhalov.FatNetLib.Microtypes;
using Kolyhalov.FatNetLib.Middlewares;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static System.Text.Encoding;

namespace Kolyhalov.FatNetLib.Modules.Json;

public class JsonDeserializationMiddleware : IMiddleware
{
    private readonly JsonSerializer _jsonSerializer;

    public JsonDeserializationMiddleware(JsonSerializer jsonSerializer)
    {
        _jsonSerializer = jsonSerializer;
    }

    public void Process(Package package)
    {
        if (package.Serialized is null)
            throw new FatNetLibException($"{nameof(package.Serialized)} field is missing");
        if (package.Schema is null)
            throw new FatNetLibException($"{nameof(package.Schema)} field is missing");
        if (package.Context is null)
            throw new FatNetLibException($"{nameof(package.Context)} field is missing");

        string packageJson = UTF8.GetString(package.Serialized);
        JObject root = JObject.Parse(packageJson);
        ParseRouteField(root, package);
        ParseIsResponseField(root, package);
        PatchSchema(package);
        ParsePackage(root, package);
    }

    private void ParseRouteField(JObject root, Package package)
    {
        JToken routeJObject = root[nameof(Package.Route)]
                              ?? throw new FatNetLibException($"{nameof(Package.Route)} field is missing");
        var route = routeJObject.ToObject<Route>(_jsonSerializer);
        package.Route = route;
    }

    private void ParseIsResponseField(JObject root, Package package)
    {
        if (!root.ContainsKey(nameof(Package.IsResponse))) return;
        JToken isResponseJObject = root[nameof(Package.IsResponse)]!;
        var isResponse = isResponseJObject.ToObject<bool>(_jsonSerializer);
        package.IsResponse = isResponse;
    }

    private static void PatchSchema(Package package)
    {
        var endpointsStorage = package.Context!.Get<IEndpointsStorage>();
        PackageSchema schemaPatch;
        if (package.IsResponse)
        {
            schemaPatch = endpointsStorage.RemoteEndpoints[package.FromPeerId!.Value]
                .First(endpoint => endpoint.Route.Equals(package.Route))
                .ResponseSchemaPatch;
        }
        else
        {
            schemaPatch = endpointsStorage.LocalEndpoints
                .First(endpoint => endpoint.EndpointData.Route.Equals(package.Route))
                .EndpointData
                .RequestSchemaPatch;
        }

        package.Schema!.Patch(schemaPatch);
    }

    private void ParsePackage(JObject root, Package package)
    {
        var fields = new Dictionary<string, object>();
        foreach (KeyValuePair<string, JToken?> node in root)
        {
            Type fieldType = package.Schema![node.Key];
            var fieldValue = node.Value!.ToObject(fieldType, _jsonSerializer)!;
            fields[node.Key] = fieldValue;
        }

        package.Fields = fields;
    }
}
