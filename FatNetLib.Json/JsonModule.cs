using System.Collections.Generic;
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
                .PutSendingMiddleware(_ => new JsonSerializationMiddleware(_.Get<JsonSerializer>()))
                .PutReceivingMiddleware(_ => new JsonDeserializationMiddleware(_.Get<JsonSerializer>()));
        }
    }
}
