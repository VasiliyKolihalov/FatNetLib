namespace UdpFramework;


public class Route : Attribute
{
    public string Path { get; }
    public Route(string path)
    {
        Path = path;
    }
}