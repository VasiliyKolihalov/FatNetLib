
namespace Kolyhalov.UdpFramework;

public class Package
{
    public string? Route { get; init; }
    public Dictionary<string, object>? Body { get; init; }

    public void Validate()
    {
        if (Route == null)
        {
            throw new UdpFrameworkException("Route is null");
        }
    }
}