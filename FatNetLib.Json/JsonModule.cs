using System.Collections.Generic;
using Kolyhalov.FatNetLib.Core;
using Kolyhalov.FatNetLib.Core.Modules;
using Newtonsoft.Json;

namespace Kolyhalov.FatNetLib.Json
{
    public class JsonModule : IModule
    {
        private readonly IEnumerable<JsonConverter>? _converters;

        public JsonModule(IEnumerable<JsonConverter>? converters = null)
        {
            _converters = converters;
        }

        public void Setup(ModuleContext moduleContext)
        {
            var jsonConverters = new List<JsonConverter>
            {
                new RouteConverter(),
                new TypeConverter(),
                new PackageSchemaConverter()
            };
            if (_converters != null)
                jsonConverters.AddRange(_converters);

            var jsonSerializer = JsonSerializer.Create(
                new JsonSerializerSettings
                {
                    Converters = jsonConverters
                });
            moduleContext.ReceivingMiddlewares.Add(new JsonDeserializationMiddleware(jsonSerializer));
            moduleContext.SendingMiddlewares.Add(new JsonSerializationMiddleware(jsonSerializer));
        }

        public IList<IModule>? ChildModules => null;
    }
}
