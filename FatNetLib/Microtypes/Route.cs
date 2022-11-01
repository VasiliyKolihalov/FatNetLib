namespace Kolyhalov.FatNetLib.Microtypes;

public class Route
{
    public readonly IReadOnlyList<string> Segments;

    private const char RouteSeparator = '/';

    public Route(string route)
    {
        if (string.IsNullOrWhiteSpace(route))
            throw new ArgumentException("Route is null or blank");

        Segments = route.Split(RouteSeparator, StringSplitOptions.RemoveEmptyEntries).ToList().AsReadOnly();
    }

    private Route()
    {
        Segments = new List<string>().AsReadOnly();
    }

    public static readonly Route Empty = new();

    public bool IsEmpty => Segments.Count is 0;

    public bool IsNotEmpty => !IsEmpty;

    public static Route operator +(Route firstRoute, Route secondRoute)
    {
        string route = firstRoute.ToString() + RouteSeparator + secondRoute;
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
        return obj.GetType() == GetType() && Equals((Route)obj);
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