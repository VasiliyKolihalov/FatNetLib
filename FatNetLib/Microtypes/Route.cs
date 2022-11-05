namespace Kolyhalov.FatNetLib.Microtypes;

public class Route
{
    public IReadOnlyList<string> Segments { get; }

    private const char RouteSeparator = '/';

    private static readonly char[] ValidCharacters =
    {
        RouteSeparator, '\'', '.', '<', '>', '!', '@', '\"', '#', '№', ';', '$', '%', ':', '^', '&', '?', '*', '-', '_',
        '=', '+'
    };

    public Route(string route)
    {
        ValidateStringRoute(route);
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

    private static void ValidateStringRoute(string route)
    {
        if (string.IsNullOrWhiteSpace(route))
            throw new ArgumentException("Route is null or blank");

        foreach (char symbol in route)
        {
            if (!char.IsLetter(symbol) && !char.IsDigit(symbol) && !ValidCharacters.Contains(symbol))
                throw new ArgumentException($"Invalid symbol in route: {symbol}");
        }
    }
}
