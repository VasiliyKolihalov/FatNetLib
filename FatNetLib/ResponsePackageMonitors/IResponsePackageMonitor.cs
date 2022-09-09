namespace Kolyhalov.FatNetLib.ResponsePackageMonitors;

public interface IResponsePackageMonitor
{
    public Package Wait(Guid exchangeId);

    public void Pulse(Package responsePackage);
}