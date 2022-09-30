namespace Kolyhalov.FatNetLib;

public class Route
{
    private readonly IReadOnlyCollection<string> _segments;

    private const char RouteSeparator = '/';

    public Route(string route)
    {
        if (string.IsNullOrWhiteSpace(route))
            throw new ArgumentException("Route is null or blank");

        _segments = route.Split(RouteSeparator, StringSplitOptions.RemoveEmptyEntries).ToList().AsReadOnly();
    }

    private Route()
    {
        _segments = new List<string>().AsReadOnly();
    }

    public static readonly Route Empty = new();

    public bool IsEmpty => _segments.Count == 0;

    public bool Contains(string routeSegment)
    {
        return _segments.Contains(routeSegment);
    }

    public static Route operator +(Route firstRoute, Route secondRoute)
    {
        string route = firstRoute.ToString() + RouteSeparator + secondRoute.ToString();
        return new Route(route);
    }

    public static Route operator +(Route route, string routePart)
    {
        return route + new Route(routePart);
    }

    public override string ToString()
    {
        return string.Join(RouteSeparator, _segments);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        return obj.GetType() == GetType() && Equals((Route) obj);
    }

    private bool Equals(Route other)
    {
        return _segments.SequenceEqual(other._segments);
    }

    public override int GetHashCode()
    {
        return _segments.GetHashCode();
    }
}