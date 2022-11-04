using Newtonsoft.Json;
using static System.Text.Encoding;

namespace Kolyhalov.FatNetLib.Middlewares;

public class JsonSerializationMiddleware : IMiddleware
{
    private readonly JsonSerializer _jsonSerializer;

    public JsonSerializationMiddleware(JsonSerializer jsonSerializer)
    {
        _jsonSerializer = jsonSerializer;
    }

    public void Process(Package package)
    {
        string packageJson = JsonConvert.SerializeObject(package.Fields, _jsonSerializer.Converters.ToArray());
        package.Serialized = UTF8.GetBytes(packageJson);
    }
}
