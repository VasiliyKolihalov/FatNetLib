namespace Kolyhalov.UdpFramework;

public class Package
{
    private readonly string? _route;
    private readonly Dictionary<string, object>? _body;

    public string? Route
    {
        get => _route;
        init
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new UdpFrameworkException("Route cannot be null or blank");
            _route = value;
        }
    }

    public Dictionary<string, object>? Body
    {
        get => _body;
        init => _body = value ?? throw new UdpFrameworkException("Body cannot be null");
    }

    public Guid? ExchangeId { get; init; }

    public bool IsResponse { get; init; }

    public Package()
    {
    }

    public Package(Package other)
    {
        _route = other._route;
        _body = other._body;
        ExchangeId = other.ExchangeId;
        IsResponse = other.IsResponse;
    }
}