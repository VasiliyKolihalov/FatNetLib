using Kolyhalov.FatNetLib.Microtypes;
using Kolyhalov.FatNetLib.Middlewares;
using Newtonsoft.Json;

namespace Kolyhalov.FatNetLib.Modules.DefaultModules;

public class JsonModule : Module
{
    public override void Setup(ModuleContext moduleContext)
    {
        var jsonSerializer = JsonSerializer.Create(
            new JsonSerializerSettings
            {
                Converters = new List<JsonConverter>
                {
                    new RouteConverter(),
                    new TypeConverter(),
                    new PackageSchemaConverter()
                }
            });
        moduleContext.ReceivingMiddlewares.Add(new JsonDeserializationMiddleware(jsonSerializer));
        moduleContext.SendingMiddlewares.Add(new JsonSerializationMiddleware(jsonSerializer));
    }
}