using Kolyhalov.FatNetLib.Microtypes;
using LiteNetLib;

namespace Kolyhalov.FatNetLib;

public class Package
{
    public IDictionary<string, object> Fields { get; set; } = new Dictionary<string, object>();
    public IDictionary<string, object> NonSendingFields { get; set; } = new Dictionary<string, object>();

    public Route? Route
    {
        get => GetField<Route>(nameof(Route));
        set => SetField(nameof(Route), value);
    }
    
    public IDictionary<string, object>? Body
    {
        get => GetField<IDictionary<string, object>?>(nameof(Body));
        set => SetField<IDictionary<string, object>?>(nameof(Body), value);
    }

    public T? GetBodyAs<T>()
    {
        return (T?)Body;
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

    public byte[]? Serialized
    {
        get => GetNonSendingField<byte[]>(nameof(Serialized));
        set => SetNonSendingField(nameof(Serialized), value);
    }

    public PackageSchema? Schema
    {
        get => GetNonSendingField<PackageSchema>(nameof(Schema));
        set => SetNonSendingField(nameof(Schema), value);
    }

    public IDependencyContext? Context
    {
        get => GetNonSendingField<IDependencyContext>(nameof(Context));
        set => SetNonSendingField(nameof(Context), value);
    }

    public int? FromPeerId
    {
        get => GetNonSendingField<int?>(nameof(FromPeerId));
        set => SetNonSendingField(nameof(FromPeerId), value);
    }

    public int? ToPeerId
    {
        get => GetNonSendingField<int?>(nameof(ToPeerId));
        set => SetNonSendingField(nameof(ToPeerId), value);
    }

    public DeliveryMethod? DeliveryMethod
    {
        get => GetNonSendingField<DeliveryMethod?>(nameof(DeliveryMethod));
        set => SetNonSendingField(nameof(DeliveryMethod), value);
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

    public T? GetNonSendingField<T>(string key)
    {
        return NonSendingFields.ContainsKey(key) ? (T)NonSendingFields[key] : default;
    }

    public void SetNonSendingField<T>(string key, T? value)
    {
        if (value == null)
        {
            RemovePrivateField(key);
        }
        else
        {
            NonSendingFields[key] = value;
        }
    }

    public void RemovePrivateField(string key)
    {
        NonSendingFields.Remove(key);
    }
}