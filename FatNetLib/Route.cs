using Newtonsoft.Json;

namespace Kolyhalov.FatNetLib;

public class Route
{
    public List<string> Value { get; }

    private const char RouteSeparator = '/';

    public Route()
    {
        Value = new List<string>();
    }

    public Route(string route)
    {
        if (string.IsNullOrWhiteSpace(route))
            throw new ArgumentException("Route is null or blank");

        List<string> value = route.Split(RouteSeparator, StringSplitOptions.RemoveEmptyEntries).ToList();

        if (value.Count == 0)
            throw new ArgumentException("Route cannot contain only slashes");

        Value = value;
    }

    public Route(Route route)
    {
        Value = route.Value.ToList();
    }
    
    [JsonIgnore]
    public bool IsEmpty => Value.Count == 0;

    public bool Contains(string routePart)
    {
        return Value.Contains(routePart);
    }

    public static Route operator +(Route firstRoute, Route secondRoute)
    {
        firstRoute.Value.AddRange(secondRoute.Value);
        return firstRoute;
    }

    public static Route operator +(Route route, string routePart)
    {
        return route + new Route(routePart);
    }

    public override string ToString()
    {
        var route = string.Empty;
        foreach (string part in Value)
        {
            route += part + RouteSeparator;
        }
        return route;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        return obj.GetType() == GetType() && Equals((Route) obj);
    }

    private bool Equals(Route other)
    {
        return Value.SequenceEqual(other.Value);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }
}