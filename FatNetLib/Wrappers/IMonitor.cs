namespace Kolyhalov.FatNetLib.Wrappers;

public interface IMonitor
{
    public WaitingResult Wait(object monitorObject, TimeSpan timeout);

    public void Pulse(object monitorObject);
}