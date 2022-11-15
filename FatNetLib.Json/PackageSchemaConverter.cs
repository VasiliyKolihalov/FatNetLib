using System;
using System.Collections.Generic;
using System.Linq;
using Kolyhalov.FatNetLib.Core.Configurations;
using Kolyhalov.FatNetLib.Core.Exceptions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kolyhalov.FatNetLib.Json
{
    public class PackageSchemaConverter : JsonConverter<PackageSchema>
    {
        public override void WriteJson(JsonWriter writer, PackageSchema? value, JsonSerializer serializer)
        {
            if (value is null)
            {
                writer.WriteNull();
                return;
            }

            writer.WriteStartObject();
            foreach (KeyValuePair<string, Type> keyTypePair in value)
            {
                writer.WritePropertyName(keyTypePair.Key);
                JsonConverter[] jsonConverters = serializer.Converters.ToArray();
                JToken.FromObject(keyTypePair.Value, serializer)
                    .WriteTo(writer, jsonConverters);
            }

            writer.WriteEndObject();
        }

        // Todo: make a correct state machine
        public override PackageSchema? ReadJson(
            JsonReader reader,
            Type objectType,
            PackageSchema? existingValue,
            bool hasExistingValue,
            JsonSerializer serializer)
        {
            var schema = new PackageSchema();
            string key = null!;
            do
            {
                switch (reader.TokenType)
                {
                    case JsonToken.StartObject:
                        break;
                    case JsonToken.PropertyName:
                        key = (reader.Value as string)!;
                        break;
                    case JsonToken.String:
                        schema[key] = JToken.ReadFrom(reader)
                            .ToObject<Type>(serializer)!;
                        break;
                    case JsonToken.EndObject:
                        return schema;
                    case JsonToken.Null:
                        return null;
                    default:
                        throw new FatNetLibException($"Unexpected token {reader.TokenType}");
                }
            } while (reader.Read());

            throw new FatNetLibException("Unexpected reader end");
        }
    }
}
