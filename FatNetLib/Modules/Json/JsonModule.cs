using Newtonsoft.Json;

namespace Kolyhalov.FatNetLib.Modules.Json;

public class JsonModule : IModule
{
    public void Setup(ModuleContext moduleContext)
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

    public IList<IModule>? ChildModules => null;
}
