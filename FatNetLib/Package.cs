namespace Kolyhalov.FatNetLib;

public class Package
{
    public IDictionary<string, object> Fields { get; set; } = new Dictionary<string, object>();
    public IDictionary<string, object> PrivateFields { get; set; } = new Dictionary<string, object>();

    public string? Route
    {
        get => GetField<string>(nameof(Route));
        set => SetField(nameof(Route), value);
    }

    public IDictionary<string, object>? Body
    {
        get => GetField<IDictionary<string, object>?>(nameof(Body));
        set => SetField<IDictionary<string, object>?>(nameof(Body), value);
    }

    public Guid ExchangeId
    {
        get => GetField<Guid>(nameof(ExchangeId));
        set => SetField(nameof(ExchangeId), value);
    }

    public bool IsResponse
    {
        get => GetField<bool>(nameof(IsResponse));
        set => SetField(nameof(IsResponse), value);
    }

    // Todo: change its type: string -> byte[] 
    public string? Serialized
    {
        get => GetPrivateField<string>(nameof(Serialized));
        set => SetPrivateField(nameof(Serialized), value);
    }
    
    public PackageSchema? Schema
    {
        get => GetPrivateField<PackageSchema>(nameof(Schema));
        set => SetPrivateField(nameof(Schema), value);
    }

    public object? this[string key]
    {
        get => GetField<object>(key);
        set => SetField(key, value);
    }

    public T? GetField<T>(string key)
    {
        return Fields.ContainsKey(key) ? (T)Fields[key] : default;
    }

    public void SetField<T>(string key, T? value)
    {
        if (value == null)
        {
            RemoveField(key);
        }
        else
        {
            Fields[key] = value;
        }
    }

    public void RemoveField(string key)
    {
        Fields.Remove(key);
    }

    public T? GetPrivateField<T>(string key)
    {
        return PrivateFields.ContainsKey(key) ? (T)PrivateFields[key] : default;
    }

    public void SetPrivateField<T>(string key, T? value)
    {
        if (value == null)
        {
            RemovePrivateField(key);
        }
        else
        {
            PrivateFields[key] = value;
        }
    }

    public void RemovePrivateField(string key)
    {
        PrivateFields.Remove(key);
    }
}