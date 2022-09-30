using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kolyhalov.FatNetLib;

public class RouteConverter : JsonConverter<Route>
{
    public override void WriteJson(JsonWriter writer, Route? value, JsonSerializer serializer)
    {
        writer.WriteValue(value!.ToString());
    }

    public override Route ReadJson(JsonReader reader, Type objectType, Route? existingValue, bool hasExistingValue,
        JsonSerializer serializer)
    {
        return new Route((string)reader.Value!);
    }
}