namespace Kolyhalov.FatNetLib;

public class Package
{

    private readonly IDictionary<string, object> _fields = new Dictionary<string, object>();

    public string? Route
    {
        get => GetField<string>(Keys.Route);
        set => SetField(Keys.Route, value);
    }

    public IDictionary<string, object>? Body
    {
        get => GetField<IDictionary<string, object>?>(Keys.Body);
        set => SetField<IDictionary<string, object>?>(Keys.Body, value);
    }

    public Guid? ExchangeId
    {
        get
        {
            var exchangeId = GetField<Guid>(Keys.ExchangeId);
            return exchangeId == default ? null : exchangeId;
        }
        set => SetField(Keys.ExchangeId, value);
    }

    public bool IsResponse
    {
        get => GetField<bool>(Keys.IsResponse);
        set => SetField(Keys.IsResponse, value);
    }

    public object? this[string key]
    {
        get => GetField<object>(key);
        set => SetField(key, value);
    }

    public T? GetField<T>(string key)
    {
        return _fields.ContainsKey(key) ? (T)_fields[key] : default;
    }

    public void SetField<T>(string key, T? value)
    {
        if (value == null)
        {
            RemoveField(key);
        }
        else
        {
            _fields[key] = value;
        }
    }

    public void RemoveField(string key)
    {
        _fields.Remove(key);
    }
    
    public static class Keys
    {
        public const string Route = "Route";
        public const string Body = "Body";
        public const string ExchangeId = "ExchangeId";
        public const string IsResponse = "IsResponse";
    }
}