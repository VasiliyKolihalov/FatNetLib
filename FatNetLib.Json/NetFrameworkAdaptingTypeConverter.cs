using System;
using Newtonsoft.Json;

namespace Kolyhalov.FatNetLib.Json
{
    // If you have .NET Framework on the local side and .NET Core on the remote side, use this converter on the local
    // side only.
    public class NetFrameworkAdaptingTypeConverter : JsonConverter<Type>
    {
        private const string NetFrameworkCoreLibName = "mscorelib";
        private const string NetCoreCoreLibName = "System.Private.CoreLib";

        public override void WriteJson(JsonWriter writer, Type? value, JsonSerializer serializer)
        {
            if (value is null)
            {
                writer.WriteNull();
                return;
            }

            string assembly = value.Assembly.GetName().Name == NetFrameworkCoreLibName
                ? NetCoreCoreLibName
                : value.Assembly.GetName().Name;

            writer.WriteValue(value.FullName + "," + assembly);
        }

        public override Type? ReadJson(
            JsonReader reader,
            Type objectType,
            Type? existingValue,
            bool hasExistingValue,
            JsonSerializer serializer)
        {
            var value = (string?)reader.Value;
            if (value is null) return null;
            if (value.EndsWith(NetCoreCoreLibName))
                value = value.TrimEnd(NetCoreCoreLibName.ToCharArray()) + NetFrameworkCoreLibName;

            return Type.GetType(value, throwOnError: true);
        }
    }
}
