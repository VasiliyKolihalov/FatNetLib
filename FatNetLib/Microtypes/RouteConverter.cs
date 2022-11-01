using Newtonsoft.Json;

namespace Kolyhalov.FatNetLib.Microtypes;

public class RouteConverter : JsonConverter<Route>
{
    public override void WriteJson(JsonWriter writer, Route? value, JsonSerializer serializer)
    {
        if (value is null)
        {
            writer.WriteValue(Route.Empty);
            return;
        }

        writer.WriteValue(value.ToString());
    }

    public override Route ReadJson(
        JsonReader reader,
        Type objectType,
        Route? existingValue,
        bool hasExistingValue,
        JsonSerializer serializer)
    {
        return new Route(reader.Value!.ToString()!);
    }
}