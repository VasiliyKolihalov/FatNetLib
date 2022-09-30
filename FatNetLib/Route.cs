namespace Kolyhalov.FatNetLib;

public class Route
{
    public IReadOnlyCollection<string> Segments { get; }

    private const char RouteSeparator = '/';

    public Route(string route)
    {
        if (string.IsNullOrWhiteSpace(route))
            throw new ArgumentException("Route is null or blank");

        Segments = route.Split(RouteSeparator, StringSplitOptions.RemoveEmptyEntries).ToList().AsReadOnly();
    }

    public Route()
    {
        Segments = new List<string>().AsReadOnly();
    }

    public static readonly Route Empty = new();

    public bool IsEmpty => Segments.Count == 0;

    public bool Contains(string routeSegment)
    {
        return Segments.Contains(routeSegment);
    }

    public static Route operator +(Route firstRoute, Route secondRoute)
    {
        string route = firstRoute.ToString() + secondRoute.ToString();
        return new Route(route);
    }

    public static Route operator +(Route route, string routePart)
    {
        return route + new Route(routePart);
    }

    public override string ToString()
    {
        return string.Join(RouteSeparator, Segments);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        return obj.GetType() == GetType() && Equals((Route) obj);
    }

    private bool Equals(Route other)
    {
        return Segments.SequenceEqual(other.Segments);
    }

    public override int GetHashCode()
    {
        return Segments.GetHashCode();
    }
}