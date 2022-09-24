using Newtonsoft.Json;

namespace Kolyhalov.FatNetLib.Middlewares;

public class SerializationMiddleware : IMiddleware
{
    public void Process(Package package)
    {
        package.Serialized = JsonConvert.SerializeObject(package.Fields);
    }
}