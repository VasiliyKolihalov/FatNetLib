using static System.AttributeTargets;

namespace Kolyhalov.FatNetLib.Attributes;

[AttributeUsage(Class | Method)]
public class Route : Attribute
{
    public string Path { get; }
    public Route(string path)
    {
        Path = path;
    }
}