namespace Kolyhalov.UdpFramework.ResponsePackageMonitors;

public interface IResponsePackageMonitorStorage
{
    public Dictionary<Guid, Package> ResponsePackages { get; }
    
    public Dictionary<Guid, object> MonitorsObjects { get; }
}