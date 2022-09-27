namespace Kolyhalov.FatNetLib;

public class Path
{
    public IList<string> Value { get; }

    private static readonly char[] PathSeparator = { '/', '\\' };

    public Path()
    {
        Value = new List<string>();
    }

    public Path(string route)
    {
        if (string.IsNullOrWhiteSpace(route))
            throw new ArgumentException("Path cannot be is null or blank");

        List<string> value = route.Split(PathSeparator, StringSplitOptions.RemoveEmptyEntries).ToList();

        if (value.Count == 0)
            throw new ArgumentException("Path cannot contain only slashes");

        Value = value;
    }

    public Path(IList<string> value)
    {
        if (value.Count == 0)
            throw new ArgumentException("Path cannot be empty");

        if (PathSeparator.Any(separator => value.Contains(separator.ToString())))
            throw new ArgumentException("Path cannot contain slashes");

        Value = value;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        return obj.GetType() == GetType() && Equals((Path) obj);
    }

    private bool Equals(Path other)
    {
        return Value.OrderBy(_ => _).SequenceEqual(other.Value.OrderBy(_ => _));
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }
}