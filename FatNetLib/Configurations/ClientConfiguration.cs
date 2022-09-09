using Kolyhalov.FatNetLib.Microtypes;

namespace Kolyhalov.FatNetLib.Configurations;

public class ClientConfiguration : Configuration
{
    public string Address { get; }

    public ClientConfiguration(string address,
        Port port,
        string connectionKey,
        Frequency? framerate,
        TimeSpan? exchangeTimeout) : base(port, connectionKey, framerate, exchangeTimeout)
    {
        Address = address ?? throw new FatNetLibException("Address cannot be null");
    }
}