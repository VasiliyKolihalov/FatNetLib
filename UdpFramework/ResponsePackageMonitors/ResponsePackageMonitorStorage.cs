namespace Kolyhalov.UdpFramework.ResponsePackageMonitors;

public class ResponsePackageMonitorStorage : IResponsePackageMonitorStorage
{
    // todo: research, how to send values through wait-pulse in monitor
    // todo: maybe try using monitor as a data container?
    public Dictionary<Guid, Package> ResponsePackages { get; } = new();
    public Dictionary<Guid, object> MonitorsObjects { get; } = new();
}