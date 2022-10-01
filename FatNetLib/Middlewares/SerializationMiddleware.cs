using Newtonsoft.Json;

namespace Kolyhalov.FatNetLib.Middlewares;

public class SerializationMiddleware : IMiddleware
{
    private readonly JsonSerializer _jsonSerializer;

    public SerializationMiddleware(JsonSerializer jsonSerializer)
    {
        _jsonSerializer = jsonSerializer;
    }
    
    public void Process(Package package)
    {
        package.Serialized = JsonConvert.SerializeObject(package.Fields, _jsonSerializer.Converters.ToArray());
    }
}