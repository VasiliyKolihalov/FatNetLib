using Newtonsoft.Json;

namespace Kolyhalov.FatNetLib;

public class TypeConverter : JsonConverter<Type>
{
    public override void WriteJson(JsonWriter writer, Type? value, JsonSerializer serializer)
    {
        if (value is null)
        {
            writer.WriteNull();
            return;
        }

        writer.WriteValue(value.FullName + "," + value.Assembly.GetName().Name);
    }

    public override Type? ReadJson(
        JsonReader reader,
        Type objectType,
        Type? existingValue,
        bool hasExistingValue,
        JsonSerializer serializer)
    {
        var value = (string?)reader.Value;
        return value is null
            ? null
            : Type.GetType(value, throwOnError: true);
    }
}
