using System;
using System.Collections.Generic;
using System.Linq;
using Kolyhalov.FatNetLib.Core.Configurations;
using Kolyhalov.FatNetLib.Core.Exceptions;
using Kolyhalov.FatNetLib.Core.Microtypes;
using Kolyhalov.FatNetLib.Core.Middlewares;
using Kolyhalov.FatNetLib.Core.Models;
using Kolyhalov.FatNetLib.Core.Storages;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static System.Text.Encoding;

namespace Kolyhalov.FatNetLib.Json
{
    public class JsonDeserializationMiddleware : IMiddleware
    {
        private readonly JsonSerializer _jsonSerializer;

        public JsonDeserializationMiddleware(JsonSerializer jsonSerializer)
        {
            _jsonSerializer = jsonSerializer;
        }

        public void Process(Package package)
        {
            if (package.Context is null)
                throw new FatNetLibException($"Package must contain {nameof(package.Context)} field");
            if (package.Schema is null)
                throw new FatNetLibException($"Package must contain {nameof(package.Schema)} field");
            if (package.Serialized is null)
                throw new FatNetLibException($"Package must contain {nameof(package.Serialized)} field");

            string packageJson = UTF8.GetString(package.Serialized!);
            JObject root = JObject.Parse(packageJson);
            ParseRouteField(root, package);
            ParseIsResponseField(root, package);
            ParseSchemaPatch(root, package);
            PatchSchema(package);
            ParsePackage(root, package);
        }

        private void ParseRouteField(JObject root, Package package)
        {
            JToken routeJToken = root[nameof(Package.Route)]
                                 ?? throw new FatNetLibException($"{nameof(Package.Route)} field is missing");
            package.Route = routeJToken.ToObject<Route>(_jsonSerializer);
        }

        private void ParseIsResponseField(JObject root, Package package)
        {
            JToken isResponseJToken = root[nameof(Package.IsResponse)]
                                      ?? throw new FatNetLibException($"{nameof(Package.IsResponse)} field is missing");
            package.IsResponse = isResponseJToken.ToObject<bool>(_jsonSerializer);
        }

        private void ParseSchemaPatch(JObject root, Package package)
        {
            JToken? schemaPatchJToken = root[nameof(Package.SchemaPatch)];
            if (schemaPatchJToken is null)
                return;
            package.SchemaPatch = schemaPatchJToken.ToObject<PackageSchema>(_jsonSerializer);
        }

        private static void PatchSchema(Package package)
        {
            var endpointsStorage = package.Context!.Get<IEndpointsStorage>();
            PackageSchema endpointPatch;
            if (package.IsResponse)
            {
                endpointPatch = endpointsStorage.RemoteEndpoints[package.Sender!.Id]
                    .First(endpoint => endpoint.Route.Equals(package.Route))
                    .ResponseSchemaPatch;
            }
            else
            {
                endpointPatch = endpointsStorage.LocalEndpoints
                    .First(endpoint => endpoint.Details.Route.Equals(package.Route))
                    .Details
                    .RequestSchemaPatch;
            }

            package.Schema!.Patch(endpointPatch);

            PackageSchema? oneTimePatch = package.SchemaPatch;
            if (oneTimePatch is null)
                return;
            package.Schema!.Patch(oneTimePatch);
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
}
