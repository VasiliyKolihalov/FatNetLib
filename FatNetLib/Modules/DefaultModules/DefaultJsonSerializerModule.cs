using Kolyhalov.FatNetLib.Microtypes;
using Kolyhalov.FatNetLib.Middlewares;
using Newtonsoft.Json;

namespace Kolyhalov.FatNetLib.Modules.DefaultModules;

public class DefaultJsonSerializerModule : IModule
{
    private static readonly JsonSerializer JsonSerializer = JsonSerializer.Create(
        new JsonSerializerSettings
        {
            Converters = new List<JsonConverter>
            {
                new RouteConverter(),
                new TypeConverter(),
                new PackageSchemaConverter()
            }
        });

    public void Setup(ModuleContext moduleContext)
    {
        moduleContext.ReceivingMiddlewares.Add(new JsonDeserializationMiddleware(JsonSerializer));
        moduleContext.SendingMiddlewares.Add(new JsonSerializationMiddleware(JsonSerializer));
    }
}