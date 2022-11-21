using System.Collections.Generic;
using Kolyhalov.FatNetLib.Core.Middlewares;
using Kolyhalov.FatNetLib.Core.Modules;
using Newtonsoft.Json;

namespace Kolyhalov.FatNetLib.Json
{
    public class JsonModule : IModule
    {
        public void Setup(IModuleContext moduleContext)
        {
            moduleContext
                .PutDependency<IList<JsonConverter>>(_ => new List<JsonConverter>
                {
                    new RouteConverter(),
                    new TypeConverter(),
                    new PackageSchemaConverter()
                })
                .PutDependency(_ => JsonSerializer.Create(
                    new JsonSerializerSettings
                    {
                        Converters = _.Get<IList<JsonConverter>>()
                    }))
                .PutScript("RegisterMiddlewares", _ =>
                {
                    var jsonSerializer = _.Get<JsonSerializer>();
                    _.Get<IList<IMiddleware>>("SendingMiddlewares")
                        .Add(new JsonSerializationMiddleware(jsonSerializer));
                    _.Get<IList<IMiddleware>>("ReceivingMiddlewares")
                        .Add(new JsonDeserializationMiddleware(jsonSerializer));
                });
        }
    }
}
