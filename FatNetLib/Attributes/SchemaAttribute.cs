using static System.AttributeTargets;

namespace Kolyhalov.FatNetLib.Attributes;

[AttributeUsage(validOn: Method | ReturnValue, AllowMultiple = true)]
public class SchemaAttribute : Attribute
{
    public string Key { get; }

    public Type Type { get; }

    public SchemaAttribute(string key, Type type)
    {
        Key = key;
        Type = type;
    }
}
