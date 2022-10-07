namespace Kolyhalov.FatNetLib.Monitors;

public interface IResponsePackageMonitor
{
    public Package Wait(Guid exchangeId);

    public void Pulse(Package responsePackage);
}