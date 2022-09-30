using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Kolyhalov.FatNetLib.Middlewares;

public class DeserializationMiddleware : IMiddleware
{
    private readonly JsonSerializer _jsonSerializer;

    public DeserializationMiddleware(JsonSerializer jsonSerializer)
    {
        _jsonSerializer = jsonSerializer;
    }

    public void Process(Package package)
    {
        if (package.Serialized == null)
            throw new FatNetLibException($"{nameof(package.Serialized)} field is missing");
        if (package.Schema == null)
            throw new FatNetLibException($"{nameof(package.Schema)} field is missing");

        JObject root = JObject.Parse(package.Serialized);
        var fields = new Dictionary<string, object>();

        foreach (KeyValuePair<string, JToken?> node in root)
        {
            Type fieldType = package.Schema[node.Key];
            var fieldValue = node.Value!.ToObject(fieldType, _jsonSerializer)!;
            fields[node.Key] = fieldValue;
        }

        package.Fields = fields;
    }
}